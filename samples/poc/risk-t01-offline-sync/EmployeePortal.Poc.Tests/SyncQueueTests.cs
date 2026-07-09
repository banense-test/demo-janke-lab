using EmployeePortal.Poc.Application;
using EmployeePortal.Poc.Domain;
using EmployeePortal.Poc.Infrastructure;
using Xunit;

namespace EmployeePortal.Poc.Tests;

/// <summary>
/// Unit tests for SyncQueue — dual coverage (black-box + white-box).
/// Black-box: validates specification (enqueue persists, flush syncs, conflict detection skips duplicates).
/// White-box: exercises every branch — null input, empty flush, conflict path, failure path, lock semantics.
/// Traces to: TC-001, ACL-013 (SyncQueue), RISK-T01.
/// </summary>
public class SyncQueueTests : IDisposable
{
    private readonly SqliteLocalStore _localStore;
    private readonly InMemoryClockingRepository _remoteRepo;
    private readonly SyncQueue _syncQueue;

    public SyncQueueTests()
    {
        _localStore = new SqliteLocalStore();
        _remoteRepo = new InMemoryClockingRepository();
        _syncQueue = new SyncQueue(_localStore, _remoteRepo);
    }

    // === BLACK-BOX TESTS: What it does (specification behavior) ===

    [Fact]
    public async Task EnqueueAsync_ValidClocking_ReturnsSuccessWithLocalId()
    {
        var clocking = new Clocking(Guid.NewGuid(), ClockingType.IN, DateTime.UtcNow, "OFFLINE");

        var result = await _syncQueue.EnqueueAsync(clocking);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value >= 1);
    }

    [Fact]
    public async Task FlushAsync_PendingClockings_SyncsToRemote()
    {
        var empId = Guid.NewGuid();
        await _syncQueue.EnqueueAsync(new Clocking(empId, ClockingType.IN, DateTime.UtcNow, "OFFLINE"));
        await _syncQueue.EnqueueAsync(new Clocking(empId, ClockingType.OUT, DateTime.UtcNow.AddSeconds(1), "OFFLINE"));

        var flushResult = await _syncQueue.FlushAsync();

        Assert.Equal(2, flushResult.TotalProcessed);
        Assert.Equal(2, flushResult.Synced);
        Assert.Equal(0, flushResult.Skipped);
        Assert.Equal(0, flushResult.Failed);
        Assert.Equal(2, _remoteRepo.GetAll().Count);
    }

    [Fact]
    public async Task FlushAsync_DuplicateClocking_MarksAsSkipped()
    {
        var empId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var clocking = new Clocking(empId, ClockingType.IN, timestamp, "OFFLINE");

        // Pre-populate remote with same clocking (simulates duplicate)
        await _remoteRepo.SaveAsync(new Clocking(empId, ClockingType.IN, timestamp, "ONLINE"));

        await _syncQueue.EnqueueAsync(clocking);

        var flushResult = await _syncQueue.FlushAsync();

        Assert.Equal(1, flushResult.TotalProcessed);
        Assert.Equal(0, flushResult.Synced);
        Assert.Equal(1, flushResult.Skipped);
    }

    [Fact]
    public async Task GetPendingCountAsync_ReturnsCorrectCount()
    {
        Assert.Equal(0, await _syncQueue.GetPendingCountAsync());

        await _syncQueue.EnqueueAsync(new Clocking(Guid.NewGuid(), ClockingType.IN, DateTime.UtcNow, "OFFLINE"));
        Assert.Equal(1, await _syncQueue.GetPendingCountAsync());

        await _syncQueue.EnqueueAsync(new Clocking(Guid.NewGuid(), ClockingType.IN, DateTime.UtcNow, "OFFLINE"));
        Assert.Equal(2, await _syncQueue.GetPendingCountAsync());
    }

    [Fact]
    public async Task FlushAsync_NoPendingRecords_ReturnsZeroResult()
    {
        var result = await _syncQueue.FlushAsync();

        Assert.Equal(0, result.TotalProcessed);
        Assert.Equal(0, result.Synced);
    }

    // === WHITE-BOX TESTS: How it does it (branch/decision coverage) ===

    [Fact]
    public async Task EnqueueAsync_NullClocking_ReturnsFailure()
    {
        var result = await _syncQueue.EnqueueAsync(null!);

        Assert.False(result.IsSuccess);
        Assert.Contains("cannot be null", result.Error);
    }

    [Fact]
    public async Task FlushAsync_RemoteSaveFails_MarksAsFailed()
    {
        var clocking = new Clocking(Guid.NewGuid(), ClockingType.IN, DateTime.UtcNow, "OFFLINE");
        await _syncQueue.EnqueueAsync(clocking);

        _remoteRepo.SetFailureMode(true);

        var flushResult = await _syncQueue.FlushAsync();

        Assert.Equal(1, flushResult.TotalProcessed);
        Assert.Equal(0, flushResult.Synced);
        Assert.Equal(0, flushResult.Skipped);
        Assert.Equal(1, flushResult.Failed);
    }

    [Fact]
    public async Task FlushAsync_MixedRecords_SyncsAndSkips()
    {
        var empId = Guid.NewGuid();
        var ts1 = DateTime.UtcNow;
        var ts2 = ts1.AddSeconds(1);

        // Pre-populate remote with first clocking (will be duplicate)
        await _remoteRepo.SaveAsync(new Clocking(empId, ClockingType.IN, ts1, "ONLINE"));

        // Enqueue both — first is duplicate, second is new
        await _syncQueue.EnqueueAsync(new Clocking(empId, ClockingType.IN, ts1, "OFFLINE"));
        await _syncQueue.EnqueueAsync(new Clocking(empId, ClockingType.OUT, ts2, "OFFLINE"));

        var flushResult = await _syncQueue.FlushAsync();

        Assert.Equal(2, flushResult.TotalProcessed);
        Assert.Equal(1, flushResult.Synced);
        Assert.Equal(1, flushResult.Skipped);
    }

    [Fact]
    public async Task FlushAsync_AfterFlush_PendingCountIsZero()
    {
        await _syncQueue.EnqueueAsync(new Clocking(Guid.NewGuid(), ClockingType.IN, DateTime.UtcNow, "OFFLINE"));
        await _syncQueue.EnqueueAsync(new Clocking(Guid.NewGuid(), ClockingType.IN, DateTime.UtcNow, "OFFLINE"));

        await _syncQueue.FlushAsync();

        Assert.Equal(0, await _syncQueue.GetPendingCountAsync());
    }

    [Fact]
    public async Task EnqueueAsync_ConcurrentEnqueues_AllSucceed()
    {
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => _syncQueue.EnqueueAsync(new Clocking(Guid.NewGuid(), ClockingType.IN, DateTime.UtcNow, "OFFLINE")))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.All(results, r => Assert.True(r.IsSuccess));
        Assert.Equal(10, await _syncQueue.GetPendingCountAsync());
    }

    [Fact]
    public async Task FlushAsync_CalledTwice_SecondCallIsEmpty()
    {
        await _syncQueue.EnqueueAsync(new Clocking(Guid.NewGuid(), ClockingType.IN, DateTime.UtcNow, "OFFLINE"));

        await _syncQueue.FlushAsync();
        var secondFlush = await _syncQueue.FlushAsync();

        Assert.Equal(0, secondFlush.TotalProcessed);
    }

    public void Dispose()
    {
        _syncQueue.Dispose();
        _localStore.Dispose();
    }
}
