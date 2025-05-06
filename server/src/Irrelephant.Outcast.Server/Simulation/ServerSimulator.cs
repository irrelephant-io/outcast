using System.Diagnostics;
using Irrelephant.Outcast.Server.Networking;
using Irrelephant.Outcast.Server.Simulation.Diagnostics;
using Irrelephant.Outcast.Server.Simulation.System.Networking;
using Microsoft.Extensions.Logging;
using ProtocolClient = Irrelephant.Outcast.Server.Simulation.Components.ProtocolClient;

namespace Irrelephant.Outcast.Server.Simulation;

public class ServerSimulator(
    OutcastWorld world,
    ServerNetworkService network,
    ILogger<ServerSimulator> logger
)
{
    private readonly Stopwatch _updateLoopStopwatch = new();

    private readonly Stopwatch _networkRxStopwatch = new();
    private readonly MovingAverageFloat _networkRxTime = new();

    private readonly Stopwatch _simStopwatch = new();
    private readonly MovingAverageFloat _simTime = new();

    private readonly Stopwatch _networkTxStopwatch = new();
    private readonly MovingAverageFloat _networkTxTime = new();

    public Task RunAsync(CancellationToken cancellationToken)
    {
        // 10 ticks per second
        var loopTimeTicks = TimeSpan.FromSeconds(0.1);
        return Task.Run(
            async () =>
            {
                await world.LoadWorldAsync();
                while (!cancellationToken.IsCancellationRequested)
                {
                    _updateLoopStopwatch.Restart();
                    ProcessNetworkRx();
                    ProcessWorldSim();
                    ProcessNetworkTx();
                    _updateLoopStopwatch.Stop();
                    SleepUntilTickRate(loopTimeTicks);
                }
            },
            cancellationToken
        );
    }

    private void SleepUntilTickRate(TimeSpan loopTimeTicks)
    {
        var sleepTime = loopTimeTicks - _updateLoopStopwatch.Elapsed;
        if (sleepTime > TimeSpan.Zero)
        {
            Thread.Sleep(sleepTime);
        }
        else
        {
            logger.LogWarning(
                "Tick processing took longer than fixed tick time by {DeltaTick}ms. Is the server overloaded?",
                -sleepTime.Milliseconds
            );
        }
    }

    private void ProcessWorldSim()
    {
        _simStopwatch.Restart();
        try
        {
            world.Simulate();
        }
        catch (Exception e)
        {
            logger.LogError(e, "World simulation failed.");
        }

        _simStopwatch.Stop();
        _simTime.Sample(_simStopwatch.ElapsedTicks);
    }

    private void ProcessNetworkTx()
    {
        _networkTxStopwatch.Restart();
        foreach (var client in network.ActiveClients)
        {
            client.Transmit();
        }
        _networkTxStopwatch.Stop();
        _networkTxTime.Sample(_networkTxStopwatch.ElapsedTicks);
    }

    private readonly IList<Networking.ProtocolClient> _clientsToDetach = [];

    private void ProcessNetworkRx()
    {
        _networkRxStopwatch.Restart();
        foreach (var client in network.ActiveClients)
        {
            if (client.IsReadyToDetach)
            {
                client.EnsureDetached(world);
                _clientsToDetach.Add(client);
            }
            else
            {
                client.EnsureAttached(world, network);
                client.Receive();
            }
        }

        if (_clientsToDetach.Count > 0)
        {
            foreach (var client in _clientsToDetach)
            {
                network.ActiveClients.Remove(client);
                client.Dispose();
            }
            _clientsToDetach.Clear();
        }
        _networkRxStopwatch.Stop();
        _networkRxTime.Sample(_networkTxStopwatch.ElapsedTicks);
    }
}
