using System;

namespace EmployeePortal.Poc.Domain;

/// <summary>
/// Represents a single clock-in or clock-out event.
/// Traces to: ACL-014 (Clocking entity), UC-001.
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
    /// Conflict detection key — uniqueness by (employeeId, timestamp).
    /// Used by SyncQueue to detect duplicate clockings during flush.
    /// </summary>
    public (Guid EmployeeId, DateTime Timestamp) ConflictKey => (EmployeeId, Timestamp);
}
