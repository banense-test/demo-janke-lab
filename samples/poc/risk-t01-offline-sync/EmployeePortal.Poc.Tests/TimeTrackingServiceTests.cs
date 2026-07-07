using EmployeePortal.Poc.Application;
using EmployeePortal.Poc.Domain;
using EmployeePortal.Poc.Infrastructure;
using Xunit;

namespace EmployeePortal.Poc.Tests;

/// <summary>
/// Unit tests for TimeTrackingService — dual coverage (black-box + white-box).
/// Black-box: validates online/offline clocking, sync trigger, pending count.
/// White-box: exercises every branch — empty employeeId, network UP, network DOWN,
///   transient failure fallback, sync after restore.
/// Traces to: TC-002, ACL-009 (TimeTrackingService), SEQ-001.
/// </summary>
public class TimeTrackingServiceTests : IDisposable
{
    private readonly SqliteLocalStore _localStore;
    private readonly InMemoryClockingRepository _remoteRepo;
    private readonly SyncQueue _syncQueue;
    private readonly TimeTrackingService _service;

    public TimeTrackingServiceTests()
    {
        _localStore = new SqliteLocalStore();
        _remoteRepo = new InMemoryClockingRepository();
        _syncQueue = new SyncQueue(_localStore, _remoteRepo);
        _service = new TimeTrackingService(new StaticHealthMonitor(HealthStatus.UP), _remoteRepo, _syncQueue);
    }

    // === BLACK-BOX TESTS: What it does (specification behavior) ===

    [Fact]
    public async Task ClockInAsync_NetworkUp_WritesToRemoteAndReturnsSuccess()
    {
        var empId = Guid.NewGuid();

        var result = await _service.ClockInAsync(empId);

        Assert.True(result.IsSuccess);
        Assert.Equal(ClockingType.IN, result.Value!.Type);
        Assert.Equal("ONLINE", result.Value.Source);
        Assert.Single(_remoteRepo.GetAll());
    }

    [Fact]
    public async Task ClockOutAsync_NetworkUp_WritesToRemoteAndReturnsSuccess()
    {
        var empId = Guid.NewGuid();

        var result = await _service.ClockOutAsync(empId);

        Assert.True(result.IsSuccess);
        Assert.Equal(ClockingType.OUT, result.Value!.Type);
    }

    [Fact]
    public async Task ClockInAsync_NetworkDown_EnqueuesOfflineAndReturnsSuccess()
    {
        var offlineService = new TimeTrackingService(
            new StaticHealthMonitor(HealthStatus.DOWN),
            _remoteRepo,
            _syncQueue);

        var result = await offlineService.ClockInAsync(Guid.NewGuid());

        Assert.True(result.IsSuccess);
        Assert.Equal("OFFLINE", result.Value!.Source);
        Assert.Empty(_remoteRepo.GetAll());
        Assert.Equal(1, await _service.GetPendingSyncCountAsync());
    }

    [Fact]
    public async Task SyncPendingAsync_AfterOfflineClockings_FlushesToRemote()
    {
        var offlineService = new TimeTrackingService(
            new StaticHealthMonitor(HealthStatus.DOWN),
            _remoteRepo,
            _syncQueue);

        await offlineService.ClockInAsync(Guid.NewGuid());
        await offlineService.ClockOutAsync(Guid.NewGuid());

        var flushResult = await _service.SyncPendingAsync();

        Assert.Equal(2, flushResult.Synced);
        Assert.Equal(2, _remoteRepo.GetAll().Count);
        Assert.Equal(0, await _service.GetPendingSyncCountAsync());
    }

    // === WHITE-BOX TESTS: How it does it (branch/decision coverage) ===

    [Fact]
    public async Task ClockInAsync_EmptyEmployeeId_ReturnsFailure()
    {
        var result = await _service.ClockInAsync(Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Contains("cannot be empty", result.Error);
    }

    [Fact]
    public async Task ClockOutAsync_EmptyEmployeeId_ReturnsFailure()
    {
        var result = await _service.ClockOutAsync(Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Contains("cannot be empty", result.Error);
    }

    [Fact]
    public async Task ClockInAsync_TransientFailure_FallsBackToOffline()
    {
        _remoteRepo.SetFailureMode(true);

        var result = await _service.ClockInAsync(Guid.NewGuid());

        // Should fall back to offline path
        Assert.True(result.IsSuccess);
        Assert.Equal("OFFLINE", result.Value!.Source);
        Assert.Equal(1, await _service.GetPendingSyncCountAsync());
    }

    [Fact]
    public async Task ClockInAsync_NetworkUp_RemoteSucceeds_NoPendingRecords()
    {
        await _service.ClockInAsync(Guid.NewGuid());

        Assert.Equal(0, await _service.GetPendingSyncCountAsync());
    }

    [Fact]
    public async Task ClockInAsync_NetworkDown_ZeroDataLoss()
    {
        var offlineService = new TimeTrackingService(
            new StaticHealthMonitor(HealthStatus.DOWN),
            _remoteRepo,
            _syncQueue);

        // Clock in 5 employees while offline
        for (int i = 0; i < 5; i++)
        {
            var result = await offlineService.ClockInAsync(Guid.NewGuid());
            Assert.True(result.IsSuccess);
        }

        Assert.Equal(5, await _service.GetPendingSyncCountAsync());

        // Restore network and sync
        var flushResult = await _service.SyncPendingAsync();

        Assert.Equal(5, flushResult.Synced);
        Assert.Equal(5, _remoteRepo.GetAll().Count);
        Assert.Equal(0, await _service.GetPendingSyncCountAsync());
    }

    public void Dispose()
    {
        _syncQueue.Dispose();
        _localStore.Dispose();
    }

    /// <summary>
    /// Test double for INetworkHealth — returns a fixed status.
    /// </summary>
    private sealed class StaticHealthMonitor : INetworkHealth
    {
        private readonly HealthStatus _status;

        public StaticHealthMonitor(HealthStatus status)
        {
            _status = status;
        }

        public HealthStatus CheckHealth() => _status;
    }
}
