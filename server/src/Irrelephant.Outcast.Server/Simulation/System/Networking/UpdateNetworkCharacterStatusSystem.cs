using Arch.Core;
using Arch.Core.Extensions;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Server.Simulation.Components.Behavioral;
using Irrelephant.Outcast.Server.Simulation.Components.Communication;
using Irrelephant.Outcast.Server.Simulation.Components.Data;

namespace Irrelephant.Outcast.Server.Simulation.System.Networking;

public static class UpdateNetworkCharacterStatusSystem
{
    public static void Run(World world)
    {
        var allClients = new QueryDescription()
            .WithAll<ProtocolClient, GlobalId, Transform, Movement>();

        world.Query(
            in allClients,
            (ref ProtocolClient pc, ref GlobalId id, ref Transform transform, ref Movement movement) =>
            {
                SendMoveUpdate(ref pc, ref movement, ref id, ref transform);

                foreach (var entityWithin in pc.InterestSphere.EntitiesWithin)
                {
                    ref var interestSphereMovable = ref entityWithin.TryGetRef<Movement>(out var isMovable);
                    var components = entityWithin.Get<Transform, GlobalId>();
                    if (isMovable)
                    {
                        SendMoveUpdate(ref pc, ref interestSphereMovable, ref components.t1, ref components.t0);
                    }
                }
            }
        );
    }

    private static void SendMoveUpdate(
        ref ProtocolClient receiver,
        ref Movement movement,
        ref GlobalId id,
        ref Transform transform
    )
    {
        if (movement.State.Current != MoveState.Idle || movement.State.DidStateChange)
        {
            var moveMessage = new EntityPositionUpdate(
                EntityId: id.Id,
                movement.MoveSpeed,
                transform.Position,
                transform.Rotation.Y
            );

            receiver.Network.EnqueueOutboundMessage(moveMessage);
        }

        if (movement.State is { Current: MoveState.Moving, DidStateChange: true })
        {
            if (movement.TargetPosition.HasValue)
            {
                receiver.Network.EnqueueOutboundMessage(
                    new InitiateMoveNotice(id.Id, movement.TargetPosition.Value)
                );
            }
            else if (movement.FollowEntity.HasValue)
            {
                ref var followGid = ref movement.FollowEntity.Value.TryGetRef<GlobalId>(out var gidExists);
                if (gidExists)
                {
                    receiver.Network.EnqueueOutboundMessage(
                        new InitiateFollowNotice(id.Id, followGid.Id)
                    );
                }
            }

        }
        else if (movement.State is { Current: MoveState.Idle or MoveState.Locked, DidStateChange: true })
        {
            receiver.Network.EnqueueOutboundMessage(new MoveDoneNotice(id.Id));
        }
    }
}
