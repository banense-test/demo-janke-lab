using EmployeePortal.Poc.Domain;
using EmployeePortal.Poc.Infrastructure;
using Xunit;

namespace EmployeePortal.Poc.Tests;

/// <summary>
/// Unit tests for SqliteLocalStore — dual coverage (black-box + white-box).
/// Black-box: validates save, get pending, update status, count.
/// White-box: exercises empty state, multiple records, status transitions.
/// Traces to: TC-004, COMP-I3 (Local Store), INT-003 (ILocalStore).
/// </summary>
public class SqliteLocalStoreTests : IDisposable
{
    private readonly SqliteLocalStore _store = new();

    // === BLACK-BOX TESTS: What it does (specification behavior) ===

    [Fact]
    public async Task SaveAsync_ValidClocking_ReturnsIncrementalLocalId()
    {
        var clocking1 = new Clocking(Guid.NewGuid(), ClockingType.IN, DateTime.UtcNow, "OFFLINE");
        var clocking2 = new Clocking(Guid.NewGuid(), ClockingType.OUT, DateTime.UtcNow, "OFFLINE");

        var id1 = await _store.SaveAsync(clocking1);
        var id2 = await _store.SaveAsync(clocking2);

        Assert.Equal(1, id1);
        Assert.Equal(2, id2);
    }

    [Fact]
    public async Task GetPendingAsync_AfterSave_ReturnsPendingRecords()
    {
        var empId = Guid.NewGuid();
        await _store.SaveAsync(new Clocking(empId, ClockingType.IN, DateTime.UtcNow, "OFFLINE"));

        var pending = await _store.GetPendingAsync();

        Assert.Single(pending);
        Assert.Equal(SyncStatus.PENDING, pending[0].Record.Status);
        Assert.Equal(empId, pending[0].Clocking.EmployeeId);
    }

    [Fact]
    public async Task UpdateSyncStatusAsync_ToSynced_UpdatesRecord()
    {
        var localId = await _store.SaveAsync(new Clocking(Guid.NewGuid(), ClockingType.IN, DateTime.UtcNow, "OFFLINE"));

        await _store.UpdateSyncStatusAsync(localId, SyncStatus.SYNCED);

        var pending = await _store.GetPendingAsync();
        Assert.Empty(pending);
    }

    [Fact]
    public async Task GetPendingCountAsync_ReflectsCurrentState()
    {
        Assert.Equal(0, await _store.GetPendingCountAsync());

        await _store.SaveAsync(new Clocking(Guid.NewGuid(), ClockingType.IN, DateTime.UtcNow, "OFFLINE"));
        await _store.SaveAsync(new Clocking(Guid.NewGuid(), ClockingType.IN, DateTime.UtcNow, "OFFLINE"));
        Assert.Equal(2, await _store.GetPendingCountAsync());

        await _store.UpdateSyncStatusAsync(1, SyncStatus.SYNCED);
        Assert.Equal(1, await _store.GetPendingCountAsync());
    }

    // === WHITE-BOX TESTS: How it does it (branch/decision coverage) ===

    [Fact]
    public async Task GetPendingAsync_NoRecords_ReturnsEmptyList()
    {
        var pending = await _store.GetPendingAsync();

        Assert.Empty(pending);
    }

    [Fact]
    public async Task UpdateSyncStatusAsync_ToSkipped_RemovesFromPending()
    {
        var localId = await _store.SaveAsync(new Clocking(Guid.NewGuid(), ClockingType.IN, DateTime.UtcNow, "OFFLINE"));

        await _store.UpdateSyncStatusAsync(localId, SyncStatus.SKIPPED);

        var pending = await _store.GetPendingAsync();
        Assert.Empty(pending);
    }

    [Fact]
    public async Task GetPendingAsync_ReturnsInLocalIdOrder()
    {
        await _store.SaveAsync(new Clocking(Guid.NewGuid(), ClockingType.IN, DateTime.UtcNow, "OFFLINE"));
        await _store.SaveAsync(new Clocking(Guid.NewGuid(), ClockingType.OUT, DateTime.UtcNow, "OFFLINE"));
        await _store.SaveAsync(new Clocking(Guid.NewGuid(), ClockingType.IN, DateTime.UtcNow, "OFFLINE"));

        var pending = await _store.GetPendingAsync();

        Assert.Equal(3, pending.Count);
        Assert.Equal(1, pending[0].Record.LocalId);
        Assert.Equal(2, pending[1].Record.LocalId);
        Assert.Equal(3, pending[2].Record.LocalId);
    }

    [Fact]
    public async Task SaveAsync_MultipleClockings_AllRetrievableAsPending()
    {
        for (int i = 0; i < 5; i++)
        {
            await _store.SaveAsync(new Clocking(Guid.NewGuid(), ClockingType.IN, DateTime.UtcNow, "OFFLINE"));
        }

        var pending = await _store.GetPendingAsync();
        Assert.Equal(5, pending.Count);
        Assert.Equal(5, await _store.GetPendingCountAsync());
    }

    public void Dispose()
    {
        _store.Dispose();
    }
}
