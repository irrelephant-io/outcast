using Arch.Core;
using Arch.Core.Extensions;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Server.Simulation.Components;
using Irrelephant.Outcast.Server.Simulation.Components.Behavioral;
using Irrelephant.Outcast.Server.Simulation.Components.Communication;
using Irrelephant.Outcast.Server.Simulation.Components.Data;

namespace Irrelephant.Outcast.Server.Simulation.System.Networking;

public static class ManageEntitiesInInterestSphereSystem
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
                        ref var namedEntity = ref entering.TryGetRef<EntityName>(out _);

                        var message = baseComponents.t1.IsPlayer
                            ? new SpawnPlayerEntity(
                                baseComponents.t1.Id,
                                baseComponents.t0.Position,
                                baseComponents.t0.Rotation.Y,
                                baseComponents.t1.ArchetypeId,
                                namedEntity.Name
                            )
                            : new SpawnEntity(
                                baseComponents.t1.Id,
                                baseComponents.t0.Position,
                                baseComponents.t0.Rotation.Y,
                                baseComponents.t1.ArchetypeId
                            );

                        protocolClient.Network.EnqueueOutboundMessage(message);

                        ref var health = ref entering.TryGetRef<Health>(out var hasHealth);
                        var isPlayer = entering.Has<ProtocolClient>();
                        if (hasHealth && !isPlayer)
                        {
                            protocolClient.Network.EnqueueOutboundMessage(
                                new HealthNotification(
                                    baseComponents.t1.Id,
                                    health.PercentHealth
                                )
                            );
                        }
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
