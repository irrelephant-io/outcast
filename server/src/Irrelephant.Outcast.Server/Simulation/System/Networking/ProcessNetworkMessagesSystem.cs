using System.Numerics;
using Arch.Core;
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
                    else if (message is InitiateMoveCommand move)
                    {
                        if (world.Has<Movable>(entity))
                        {
                            ref var movable = ref world.Get<Movable>(entity);
                            movable.Position = move.MovePosition;
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
            new Movable { Position = DefaultSpawnPosition, MoveSpeed = 3.0f },
            new NamedEntity { Name = request.Name }
        );

        positionTracker.Track(entity);
        protocolClient.Network.EnqueueOutboundMessage(
            new ConnectResponse
            (
                AcceptedName: request.Name,
                protocolClient.Network.SessionId,
                newEntityId,
                SpawnPosition: DefaultSpawnPosition,
                YAxisRotation: 0
            )
        );
    }
}
