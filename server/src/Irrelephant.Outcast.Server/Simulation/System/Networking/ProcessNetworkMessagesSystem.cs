using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Server.Simulation.Components;
using Irrelephant.Outcast.Server.Simulation.Space;
using Irrelephant.Outcast.Server.Simulation.System.EntityLifecycle;

namespace Irrelephant.Outcast.Server.Simulation.System.Networking;

public class ProcessNetworkMessagesSystem(
    EntitySpawner entitySpawner
)
{
    public void Run(World world)
    {
        var allClients = new QueryDescription().WithAll<ProtocolClient>();
        world.Query(
            in allClients,
            (Entity entity, ref ProtocolClient protocolClient) =>
            {
                while (protocolClient.Network.TryDequeueInboundMessage(out var message))
                {
                    if (message is InitiateMoveRequest move)
                    {
                        ProcessMoveRequest(world, ref protocolClient, ref entity, move);
                    }
                    else if (message is ConnectRequest request)
                    {
                        ProcessConnectRequest(ref protocolClient, ref entity, request);
                    }
                }
            }
        );
    }

    private static void ProcessMoveRequest(
        World world,
        ref ProtocolClient protocolClient,
        ref Entity entity,
        InitiateMoveRequest move
    )
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

    private void ProcessConnectRequest(
        ref ProtocolClient protocolClient,
        ref Entity entity,
        ConnectRequest request
    )
    {
        entitySpawner.SpawnConnectingPlayer(ref entity, request.Name);
        var gid = entity.Get<GlobalId>();
        var transform = entity.Get<Transform>();
        protocolClient.Network.EnqueueOutboundMessage(
            new ConnectResponse
            (
                protocolClient.Network.SessionId,
                gid.Id,
                SpawnPosition: transform.Position,
                YAxisRotation: transform.Rotation.Y
            )
        );
    }
}
