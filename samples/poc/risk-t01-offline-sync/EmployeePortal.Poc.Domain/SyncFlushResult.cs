using System;

namespace EmployeePortal.Poc.Domain;

/// <summary>
/// Result of a sync flush operation — counts of synced, skipped, and failed records.
/// </summary>
public sealed class SyncFlushResult
{
    public int TotalProcessed { get; init; }
    public int Synced { get; init; }
    public int Skipped { get; init; }
    public int Failed { get; init; }
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
}
