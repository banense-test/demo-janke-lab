using EmployeePortal.Poc.Application;
using EmployeePortal.Poc.Infrastructure;
using Xunit;

namespace EmployeePortal.Poc.Tests;

/// <summary>
/// Unit tests for TcpHealthMonitor — dual coverage (black-box + white-box).
/// Black-box: validates UP/DOWN detection against a real TCP listener.
/// White-box: exercises timeout path, exception path, constructor validation.
/// Traces to: TC-003, COMP-I5 (Network Health Monitor).
/// </summary>
public class TcpHealthMonitorTests
{
    // === BLACK-BOX TESTS: What it does (specification behavior) ===

    [Fact]
    public void CheckHealth_ActiveListener_ReturnsUP()
    {
        using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;

        try
        {
            var monitor = new TcpHealthMonitor("localhost", port, 3000);
            var status = monitor.CheckHealth();

            Assert.Equal(HealthStatus.UP, status);
        }
        finally
        {
            listener.Stop();
        }
    }

    [Fact]
    public void CheckHealth_NoListener_ReturnsDOWN()
    {
        // Use a port that's almost certainly not listening
        var monitor = new TcpHealthMonitor("localhost", 54399, 1000);
        var status = monitor.CheckHealth();

        Assert.Equal(HealthStatus.DOWN, status);
    }

    // === WHITE-BOX TESTS: How it does it (branch/decision coverage) ===

    [Fact]
    public void Constructor_NullHost_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TcpHealthMonitor(null!));
    }

    [Fact]
    public void Constructor_InvalidPort_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TcpHealthMonitor("localhost", 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TcpHealthMonitor("localhost", 70000));
    }

    [Fact]
    public void Constructor_InvalidTimeout_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TcpHealthMonitor("localhost", 5432, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TcpHealthMonitor("localhost", 5432, -1));
    }

    [Fact]
    public void CheckHealth_UnreachableHost_ReturnsDOWN()
    {
        // Non-routable address — should timeout/exception → DOWN
        var monitor = new TcpHealthMonitor("192.0.2.1", 5432, 500);
        var status = monitor.CheckHealth();

        Assert.Equal(HealthStatus.DOWN, status);
    }
}
