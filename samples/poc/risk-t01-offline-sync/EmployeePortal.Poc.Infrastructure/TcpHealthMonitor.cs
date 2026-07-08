using System.Net.Sockets;
using EmployeePortal.Poc.Application;

namespace EmployeePortal.Poc.Infrastructure;

/// <summary>
/// TCP-based health monitor that probes PostgreSQL availability.
/// Probes port 5432 every 5 seconds (configurable).
/// Traces to: COMP-I5 (Network Health Monitor), INT-005 (INetworkHealth).
/// CR #7 fix: Replaced sync-over-async ConnectAsync().Wait() with fully async
/// ConnectAsync() + CancellationTokenSource for timeout, eliminating thread pool
/// starvation risk under concurrent load.
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

    public async Task<HealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_timeoutMs);

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(_host, _port, cts.Token);
            return HealthStatus.UP;
        }
        catch
        {
            return HealthStatus.DOWN;
        }
    }
}
