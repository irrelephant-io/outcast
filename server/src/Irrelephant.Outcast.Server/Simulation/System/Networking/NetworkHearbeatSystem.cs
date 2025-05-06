using Arch.Core;
using Irrelephant.Outcast.Server.Simulation.Components;

namespace Irrelephant.Outcast.Server.Simulation.System.Networking;

public static class NetworkHeartbeatSystem
{
    public static void Run(World world)
    {
        var allClientsQuery = new QueryDescription().WithAll<ProtocolClient>();
        world.Query(
            in allClientsQuery,
            static (ref ProtocolClient client) =>
            {
                if (client.Network.LastNetworkActivity < DateTime.UtcNow - TimeSpan.FromSeconds(10))
                {
                    client.Network.EnqueueHeartbeatMessage();
                }
            }
        );
    }
}
