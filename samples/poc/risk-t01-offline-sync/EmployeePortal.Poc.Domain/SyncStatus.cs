namespace EmployeePortal.Poc.Domain;

/// <summary>
/// Status of a sync record in the offline-to-online flush cycle.
/// Traces to: ACL-018, SyncStatus enum.
/// </summary>
public enum SyncStatus
{
    PENDING,
    SYNCED,
    SKIPPED
}
