using System;

namespace EmployeePortal.Poc.Domain;

/// <summary>
/// Represents a single clock-in or clock-out event.
/// Traces to: ACL-014 (Clocking entity), UC-001.
/// CR #8 fix: Added internal Rehydrate() factory method to replace reflection-based
/// property setting in SqliteLocalStore — fragile pattern for .NET version upgrades.
/// </summary>
public sealed class Clocking
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public ClockingType Type { get; init; }
    public DateTime Timestamp { get; init; }
    public string Source { get; init; } = "ONLINE";

    public Clocking(Guid employeeId, ClockingType type, DateTime timestamp, string source = "ONLINE")
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("EmployeeId cannot be empty.", nameof(employeeId));
        if (timestamp == default)
            throw new ArgumentException("Timestamp must be set.", nameof(timestamp));

        Id = Guid.NewGuid();
        EmployeeId = employeeId;
        Type = type;
        Timestamp = timestamp;
        Source = source;
    }

    /// <summary>
    /// Reconstitutes a Clocking entity from persisted data without reflection.
    /// Used by SqliteLocalStore.GetPendingAsync() to hydrate entities from SQLite rows.
    /// </summary>
    internal static Clocking Rehydrate(Guid id, Guid employeeId, ClockingType type, DateTime timestamp, string source)
    {
        return new Clocking(employeeId, type, timestamp, source)
        {
            Id = id
        };
    }

    /// <summary>
    /// Conflict detection key — uniqueness by (employeeId, timestamp).
    /// Used by SyncQueue to detect duplicate clockings during flush.
    /// </summary>
    public (Guid EmployeeId, DateTime Timestamp) ConflictKey => (EmployeeId, Timestamp);
}
