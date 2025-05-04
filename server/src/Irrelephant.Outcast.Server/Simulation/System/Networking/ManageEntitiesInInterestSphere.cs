using Arch.Core;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Server.Simulation.Components;

namespace Irrelephant.Outcast.Server.Simulation.System.Networking;

public static class ManageEntitiesInInterestSphere
{
    public static void Run(World world)
    {
        var allClients = new QueryDescription().WithAll<ProtocolClient>();
        world.Query(
            in allClients,
            (ref ProtocolClient protocolClient) =>
            {
                foreach (var entering in protocolClient.InterestSphere.EnteringEntities)
                {
                    if (entering.Has<Transform, GlobalId>())
                    {
                        var components = entering.Get<Transform, GlobalId>();
                        protocolClient.Network.EnqueueOutboundMessage(
                            new SpawnEntity(
                                components.t1.Id,
                                components.t0.Position,
                                components.t0.Rotation.Y
                            )
                        );
                    }
                }

                foreach (var leaving in protocolClient.InterestSphere.LeavingEntities)
                {
                    if (leaving.Has<Transform, GlobalId>())
                    {
                        var components = leaving.Get<Transform, GlobalId>();
                        protocolClient.Network.EnqueueOutboundMessage(
                            new DespawnEntity(components.t1.Id)
                        );
                    }
                }
            }
        );
    }
}
