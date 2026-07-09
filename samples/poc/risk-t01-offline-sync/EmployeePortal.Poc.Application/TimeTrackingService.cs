using EmployeePortal.Poc.Domain;

namespace EmployeePortal.Poc.Application;

/// <summary>
/// Application service for clock-in/clock-out operations.
///
/// Key decision: when network is UP, writes directly to PostgreSQL.
/// When network is DOWN (or transient failure), falls back to SyncQueue (offline path).
/// User receives immediate confirmation in both modes (<1s, REQ-017).
///
/// Traces to: ACL-009 (TimeTrackingService), COMP-A1, SEQ-001.
/// CR #7 fix: Updated to use async CheckHealthAsync() instead of sync CheckHealth().
/// </summary>
public sealed class TimeTrackingService
{
    private readonly INetworkHealth _networkHealth;
    private readonly IRepository<Clocking> _repository;
    private readonly SyncQueue _syncQueue;

    public TimeTrackingService(
        INetworkHealth networkHealth,
        IRepository<Clocking> repository,
        SyncQueue syncQueue)
    {
        _networkHealth = networkHealth ?? throw new ArgumentNullException(nameof(networkHealth));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _syncQueue = syncQueue ?? throw new ArgumentNullException(nameof(syncQueue));
    }

    /// <summary>
    /// Records a clock-in event. Online path writes to PostgreSQL;
    /// offline path enqueues to SyncQueue for later sync.
    /// </summary>
    public async Task<Result<Clocking>> ClockInAsync(Guid employeeId)
    {
        return await ProcessClockingAsync(employeeId, ClockingType.IN);
    }

    /// <summary>
    /// Records a clock-out event. Same online/offline logic as ClockIn.
    /// </summary>
    public async Task<Result<Clocking>> ClockOutAsync(Guid employeeId)
    {
        return await ProcessClockingAsync(employeeId, ClockingType.OUT);
    }

    private async Task<Result<Clocking>> ProcessClockingAsync(Guid employeeId, ClockingType type)
    {
        if (employeeId == Guid.Empty)
            return Result<Clocking>.Fail("EmployeeId cannot be empty.");

        var clocking = new Clocking(employeeId, type, DateTime.UtcNow);

        var health = await _networkHealth.CheckHealthAsync();

        if (health == HealthStatus.UP)
        {
            // Online path: write directly to PostgreSQL
            try
            {
                bool saved = await _repository.SaveAsync(clocking);
                if (saved)
                {
                    return Result<Clocking>.Ok(clocking);
                }
                // Transient failure — fall back to offline path
            }
            catch
            {
                // Transient failure — fall back to offline path
            }
        }

        // Offline path: enqueue to local store for later sync
        clocking = new Clocking(employeeId, type, clocking.Timestamp, source: "OFFLINE");
        var enqueueResult = await _syncQueue.EnqueueAsync(clocking);
        if (enqueueResult.IsSuccess)
        {
            return Result<Clocking>.Ok(clocking);
        }

        return Result<Clocking>.Fail($"Failed to record clocking: {enqueueResult.Error}");
    }

    /// <summary>
    /// Triggers a sync flush of all pending offline clockings.
    /// Called when network health transitions from DOWN to UP.
    /// </summary>
    public async Task<SyncFlushResult> SyncPendingAsync()
    {
        return await _syncQueue.FlushAsync();
    }

    /// <summary>
    /// Returns the count of pending offline clockings awaiting sync.
    /// </summary>
    public async Task<int> GetPendingSyncCountAsync()
    {
        return await _syncQueue.GetPendingCountAsync();
    }
}
