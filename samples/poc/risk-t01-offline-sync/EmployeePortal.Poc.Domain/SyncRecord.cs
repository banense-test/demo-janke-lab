using System;

namespace EmployeePortal.Poc.Domain;

/// <summary>
/// Tracks the sync state of a locally-stored clocking.
/// Traces to: ACL-018 (SyncRecord entity), UC-001 offline flow.
/// CR #8 fix: Added internal Rehydrate() factory method to replace reflection-based
/// property setting in SqliteLocalStore — fragile pattern for .NET version upgrades.
/// </summary>
public sealed class SyncRecord
{
    public Guid Id { get; init; }
    public int LocalId { get; init; }
    public Guid ClockingId { get; init; }
    public SyncStatus Status { get; set; } = SyncStatus.PENDING;
    public DateTime QueuedAt { get; init; }
    public DateTime? SyncedAt { get; set; }

    public SyncRecord(int localId, Guid clockingId)
    {
        if (localId < 0)
            throw new ArgumentException("LocalId cannot be negative.", nameof(localId));
        if (clockingId == Guid.Empty)
            throw new ArgumentException("ClockingId cannot be empty.", nameof(clockingId));

        Id = Guid.NewGuid();
        LocalId = localId;
        ClockingId = clockingId;
        QueuedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reconstitutes a SyncRecord from persisted data without reflection.
    /// Used by SqliteLocalStore.GetPendingAsync() to hydrate entities from SQLite rows.
    /// </summary>
    internal static SyncRecord Rehydrate(Guid id, int localId, Guid clockingId, SyncStatus status, DateTime queuedAt, DateTime? syncedAt)
    {
        var record = new SyncRecord(localId, clockingId)
        {
            Id = id,
            QueuedAt = queuedAt
        };
        record.Status = status;
        record.SyncedAt = syncedAt;
        return record;
    }
}
