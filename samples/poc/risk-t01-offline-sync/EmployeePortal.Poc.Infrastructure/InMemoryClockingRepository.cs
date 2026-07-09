using EmployeePortal.Poc.Application;
using EmployeePortal.Poc.Domain;

namespace EmployeePortal.Poc.Infrastructure;

/// <summary>
/// In-memory implementation of IRepository&lt;Clocking&gt; for PoC testing.
/// Simulates the PostgreSQL repository in production.
/// Traces to: COMP-I2 (Repository), INT-002 (IRepository).
/// </summary>
public sealed class InMemoryClockingRepository : IRepository<Clocking>
{
    private readonly List<Clocking> _store = new();
    private readonly object _lock = new();
    private bool _shouldFail;

    /// <summary>
    /// Sets the repository to simulate failure on next SaveAsync call.
    /// Used for white-box testing of transient failure fallback.
    /// </summary>
    public void SetFailureMode(bool shouldFail)
    {
        _shouldFail = shouldFail;
    }

    public Task<bool> SaveAsync(Clocking entity)
    {
        if (_shouldFail)
        {
            _shouldFail = false;
            return Task.FromResult(false);
        }

        lock (_lock)
        {
            _store.Add(entity);
        }
        return Task.FromResult(true);
    }

    public Task<bool> ExistsAsync(Func<Clocking, bool> predicate)
    {
        lock (_lock)
        {
            return Task.FromResult(_store.Any(predicate));
        }
    }

    public Task<IReadOnlyList<Clocking>> QueryAsync(Func<Clocking, bool> predicate)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<Clocking>>(_store.Where(predicate).ToList().AsReadOnly());
        }
    }

    /// <summary>
    /// Returns all stored clockings (for test verification).
    /// </summary>
    public IReadOnlyList<Clocking> GetAll()
    {
        lock (_lock)
        {
            return _store.ToList().AsReadOnly();
        }
    }
}
