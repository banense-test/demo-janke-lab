using EmployeePortal.Poc.Domain;

namespace EmployeePortal.Poc.Application;

/// <summary>
/// Abstraction for the primary persistence store (PostgreSQL in production).
/// Traces to: INT-002 (IRepository&lt;T&gt;), COMP-I2 (Repository).
/// </summary>
public interface IRepository<T> where T : class
{
    Task<bool> SaveAsync(T entity);
    Task<bool> ExistsAsync(Func<T, bool> predicate);
    Task<IReadOnlyList<T>> QueryAsync(Func<T, bool> predicate);
}
