using Arch.Core;
using Arch.Core.Extensions;
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
                        var baseComponents = entering.Get<Transform, GlobalId>();
                        ref var namedEntity = ref entering.TryGetRef<NamedEntity>(out var isNamed);

                        var message = isNamed
                            ? new SpawnPlayerEntity(
                                baseComponents.t1.Id,
                                baseComponents.t0.Position,
                                baseComponents.t0.Rotation.Y,
                                namedEntity.Name
                            )
                            : new SpawnEntity(
                                baseComponents.t1.Id,
                                baseComponents.t0.Position,
                                baseComponents.t0.Rotation.Y

                            );

                        protocolClient.Network.EnqueueOutboundMessage(message);
                    }
                }

                foreach (var leaving in protocolClient.InterestSphere.LeavingEntities)
                {
                    if (leaving.Has<GlobalId>())
                    {
                        var gid = leaving.Get<GlobalId>();
                        protocolClient.Network.EnqueueOutboundMessage(
                            new DespawnEntity(gid.Id)
                        );
                    }
                }
            }
        );
    }
}
