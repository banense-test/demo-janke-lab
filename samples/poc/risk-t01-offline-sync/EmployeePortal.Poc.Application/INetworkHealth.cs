using EmployeePortal.Poc.Domain;

namespace EmployeePortal.Poc.Application;

/// <summary>
/// Abstraction for network health detection.
/// Design decision: TcpHealthMonitor probes pg:5432 every 5s.
/// Traces to: INT-005 (INetworkHealth), COMP-I5 (Network Health Monitor).
/// </summary>
public enum HealthStatus
{
    UP,
    DOWN
}

public interface INetworkHealth
{
    HealthStatus CheckHealth();
}
