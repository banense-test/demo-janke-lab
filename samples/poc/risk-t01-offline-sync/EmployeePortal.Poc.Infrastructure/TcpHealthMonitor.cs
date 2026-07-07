using System.Net.Sockets;
using EmployeePortal.Poc.Application;

namespace EmployeePortal.Poc.Infrastructure;

/// <summary>
/// TCP-based health monitor that probes PostgreSQL availability.
/// Probes port 5432 every 5 seconds (configurable).
/// Traces to: COMP-I5 (Network Health Monitor), INT-005 (INetworkHealth).
/// </summary>
public sealed class TcpHealthMonitor : INetworkHealth
{
    private readonly string _host;
    private readonly int _port;
    private readonly int _timeoutMs;

    public TcpHealthMonitor(string host = "localhost", int port = 5432, int timeoutMs = 3000)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        if (port <= 0 || port > 65535)
            throw new ArgumentOutOfRangeException(nameof(port));
        if (timeoutMs <= 0)
            throw new ArgumentOutOfRangeException(nameof(timeoutMs));

        _port = port;
        _timeoutMs = timeoutMs;
    }

    public HealthStatus CheckHealth()
    {
        try
        {
            using var client = new TcpClient();
            var connected = client.ConnectAsync(_host, _port).Wait(_timeoutMs);
            return connected && client.Connected ? HealthStatus.UP : HealthStatus.DOWN;
        }
        catch
        {
            return HealthStatus.DOWN;
        }
    }
}
