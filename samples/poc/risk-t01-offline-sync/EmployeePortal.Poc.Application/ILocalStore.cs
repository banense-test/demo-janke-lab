using EmployeePortal.Poc.Domain;

namespace EmployeePortal.Poc.Application;

/// <summary>
/// Abstraction for the local offline store (SQLite in production).
/// Traces to: INT-003 (ILocalStore), COMP-I3 (Local Store).
/// </summary>
public interface ILocalStore
{
    /// <summary>
    /// Saves a clocking locally and returns the assigned local ID.
    /// </summary>
    Task<int> SaveAsync(Clocking clocking);

    /// <summary>
    /// Retrieves all clockings that have PENDING sync status.
    /// </summary>
    Task<IReadOnlyList<(Clocking Clocking, SyncRecord Record)>> GetPendingAsync();

    /// <summary>
    /// Updates the sync status of a record.
    /// </summary>
    Task UpdateSyncStatusAsync(int localId, SyncStatus status);

    /// <summary>
    /// Gets the count of pending sync records.
    /// </summary>
    Task<int> GetPendingCountAsync();
}
