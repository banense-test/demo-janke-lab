using EmployeePortal.Poc.Domain;

namespace EmployeePortal.Poc.Application;

/// <summary>
/// Manages the offline-to-online sync lifecycle for clocking operations.
/// 
/// Key mechanisms validated by this PoC:
/// - SemaphoreSlim(1,1) single-writer lock for thread-safe enqueue
/// - Flush on network restore with conflict detection by (employeeId, timestamp)
/// - Zero data loss: every clocking is persisted locally before confirmation
/// 
/// Traces to: ACL-013 (SyncQueue), COMP-D4 (Sync Queue), RISK-T01 (RPN 63).
/// </summary>
public sealed class SyncQueue : IDisposable
{
    private readonly ILocalStore _localStore;
    private readonly IRepository<Clocking> _remoteRepo;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly SemaphoreSlim _flushLock = new(1, 1);
    private bool _disposed;

    public SyncQueue(ILocalStore localStore, IRepository<Clocking> remoteRepo)
    {
        _localStore = localStore ?? throw new ArgumentNullException(nameof(localStore));
        _remoteRepo = remoteRepo ?? throw new ArgumentNullException(nameof(remoteRepo));
    }

    /// <summary>
    /// Enqueues a clocking to the local store for later sync.
    /// Thread-safe via SemaphoreSlim(1,1) single-writer lock.
    /// </summary>
    public async Task<Result<int>> EnqueueAsync(Clocking clocking)
    {
        if (clocking == null)
            return Result<int>.Fail("Clocking cannot be null.");

        await _writeLock.WaitAsync();
        try
        {
            var localId = await _localStore.SaveAsync(clocking);
            return Result<int>.Ok(localId);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail($"Failed to enqueue clocking: {ex.Message}");
        }
        finally
        {
            _writeLock.Release();
        }
    }

    /// <summary>
    /// Flushes all pending clockings to the remote repository.
    /// Conflict detection: if a clocking with the same (employeeId, timestamp) already exists remotely,
    /// the record is marked SKIPPED (not retried).
    /// Thread-safe via separate flush lock — allows new enqueues during flush.
    /// </summary>
    public async Task<SyncFlushResult> FlushAsync()
    {
        await _flushLock.WaitAsync();
        try
        {
            var pending = await _localStore.GetPendingAsync();
            if (pending.Count == 0)
                return new SyncFlushResult { TotalProcessed = 0, Synced = 0, Skipped = 0, Failed = 0 };

            int synced = 0, skipped = 0, failed = 0;

            foreach (var (clocking, record) in pending)
            {
                // Conflict detection: check if already exists remotely by (employeeId, timestamp)
                bool exists = await _remoteRepo.ExistsAsync(c =>
                    c.EmployeeId == clocking.EmployeeId && c.Timestamp == clocking.Timestamp);

                if (exists)
                {
                    await _localStore.UpdateSyncStatusAsync(record.LocalId, SyncStatus.SKIPPED);
                    skipped++;
                    continue;
                }

                bool saved = await _remoteRepo.SaveAsync(clocking);
                if (saved)
                {
                    await _localStore.UpdateSyncStatusAsync(record.LocalId, SyncStatus.SYNCED);
                    synced++;
                }
                else
                {
                    failed++;
                }
            }

            return new SyncFlushResult
            {
                TotalProcessed = pending.Count,
                Synced = synced,
                Skipped = skipped,
                Failed = failed
            };
        }
        finally
        {
            _flushLock.Release();
        }
    }

    /// <summary>
    /// Returns the count of pending sync records.
    /// </summary>
    public async Task<int> GetPendingCountAsync()
    {
        return await _localStore.GetPendingCountAsync();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _writeLock.Dispose();
            _flushLock.Dispose();
            _disposed = true;
        }
    }
}
