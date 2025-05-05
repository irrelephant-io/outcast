using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Server.Simulation.Components;
using Irrelephant.Outcast.Server.Simulation.Space;

namespace Irrelephant.Outcast.Server.Simulation.System.Networking;

public class ProcessNetworkMessagesSystem(IPositionTracker positionTracker)
{
    private static readonly Vector3 DefaultSpawnPosition = new(467, 1168, -622);

    public void Run(World world)
    {
        var allClients = new QueryDescription().WithAll<ProtocolClient>();
        world.Query(
            in allClients,
            (Entity entity, ref ProtocolClient protocolClient) =>
            {
                while (protocolClient.Network.TryDequeueInboundMessage(out var message))
                {
                    if (message is ConnectRequest request)
                    {
                        ProcessConnectRequest(world, ref protocolClient, ref entity, request, positionTracker);
                    }
                    else if (message is InitiateMoveRequest move)
                    {
                        if (world.Has<Movable, GlobalId>(entity))
                        {
                            var movable = world.Get<Movable, GlobalId>(entity);
                            movable.t0.Position = move.MovePosition;

                            var moveNotice = new InitiateMoveNotice(
                                movable.t1.Id,
                                move.MovePosition
                            );

                            protocolClient.Network.EnqueueOutboundMessage(moveNotice);

                            // Notify other interested clients that the entity is moving.
                            foreach (var interestedEntity in protocolClient.InterestSphere.EntitiesWithin)
                            {
                                ref var client = ref interestedEntity.TryGetRef<ProtocolClient>(out var hasClient);
                                if (hasClient)
                                {
                                    client.Network.EnqueueOutboundMessage(moveNotice);
                                }
                            }
                        }
                    }
                }
            }
        );
    }

    private static void ProcessConnectRequest(
        World world,
        ref ProtocolClient protocolClient,
        ref Entity entity,
        ConnectRequest request,
        IPositionTracker positionTracker
    )
    {
        var newEntityId = Guid.NewGuid();
        world.Add(
            entity,
            new GlobalId { Id = newEntityId },
            new NetworkSessionId { Id = protocolClient.Network.SessionId },
            new Transform { Position = DefaultSpawnPosition },
            new Movable { Position = DefaultSpawnPosition, MoveSpeed = 5.0f },
            new NamedEntity { Name = request.Name }
        );

        positionTracker.Track(entity);
        protocolClient.Network.EnqueueOutboundMessage(
            new ConnectResponse
            (
                protocolClient.Network.SessionId,
                newEntityId,
                SpawnPosition: DefaultSpawnPosition,
                YAxisRotation: 0
            )
        );
    }
}
