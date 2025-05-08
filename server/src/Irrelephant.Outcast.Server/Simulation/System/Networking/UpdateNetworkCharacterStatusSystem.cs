using Arch.Core;
using Arch.Core.Extensions;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Server.Simulation.Components;

namespace Irrelephant.Outcast.Server.Simulation.System.Networking;

public static class UpdateNetworkCharacterStatus
{
    public static void Run(World world)
    {
        var allClients = new QueryDescription()
            .WithAll<ProtocolClient, GlobalId, Transform, Movable>();

        world.Query(
            in allClients,
            (ref ProtocolClient pc, ref GlobalId id, ref Transform transform, ref Movable movable) =>
            {
                if (movable.IsMoved)
                {
                    var moveMessage = new EntityPositionUpdate(
                        EntityId: id.Id,
                        movable.MoveSpeed,
                        transform.Position,
                        transform.Rotation.Y
                    );
                    var moveDoneNotice = movable.IsDoneMoving
                        ? new MoveDoneNotice(id.Id)
                        : null;

                    pc.Network.EnqueueOutboundMessage(moveMessage);
                    if (moveDoneNotice is not null)
                    {
                        pc.Network.EnqueueOutboundMessage(new MoveDoneNotice(id.Id));
                    }
                    foreach (var clientOfInterest in pc.InterestSphere.EntitiesWithin)
                    {
                        ref var client = ref clientOfInterest.TryGetRef<ProtocolClient>(out var exists);
                        if (exists)
                        {
                            client.Network.EnqueueOutboundMessage(moveMessage);
                            if (moveDoneNotice is not null)
                            {
                                client.Network.EnqueueOutboundMessage(new MoveDoneNotice(id.Id));
                            }
                        }
                    }
                }
            }
        );
    }
}
