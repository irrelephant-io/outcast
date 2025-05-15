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
            .WithAll<ProtocolClient, GlobalId, Transform>()
            .WithAny<Movement, Attack>();

        world.Query(
            in allClients,
            (Entity entity, ref ProtocolClient pc, ref GlobalId id, ref Transform transform) =>
            {
                ref var attack = ref entity.TryGetRef<Attack>(out var hasAttack);
                ref var movement = ref entity.TryGetRef<Movement>(out var hasMovement);

                if (hasMovement)
                {
                    SendMoveUpdate(ref pc, ref movement, ref id, ref transform);
                }

                if (hasAttack)
                {
                    SendOwnAttackUpdates(ref pc, ref attack, ref id);
                }

                foreach (var entityWithin in pc.InterestSphere.EntitiesWithin)
                {
                    ref var interestSphereMovement = ref entityWithin.TryGetRef<Movement>(
                        out var interestSphereHasMovement
                    );
                    ref var interestSphereAttack = ref entityWithin.TryGetRef<Attack>(
                        out var interestSphereHasAttack
                    );
                    if (interestSphereHasMovement)
                    {
                        var components = entityWithin.Get<Transform, GlobalId>();
                        SendMoveUpdate(ref pc, ref interestSphereMovement, ref components.t1, ref components.t0);
                    }

                    if (interestSphereHasAttack)
                    {
                        var gid = entityWithin.Get<GlobalId>();
                        SendAttackUpdatesFromOthers(ref entity, ref pc, ref interestSphereAttack, ref gid);
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
            var moveMessage = new EntityPositionNotification(
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
                    new MoveNotification(id.Id, movement.TargetPosition.Value)
                );
            }
            else if (movement.FollowEntity.HasValue)
            {
                ref var followGid = ref movement.FollowEntity.Value.TryGetRef<GlobalId>(out var gidExists);
                if (gidExists)
                {
                    receiver.Network.EnqueueOutboundMessage(
                        new FollowNotification(id.Id, followGid.Id)
                    );
                }
            }

        }
        else if (movement.State is {
             Current: MoveState.Idle or MoveState.Locked or MoveState.Stopped,
             DidStateChange: true
        })
        {
            receiver.Network.EnqueueOutboundMessage(new MovementStopNotification(id.Id));
        }
    }

    private static void SendOwnAttackUpdates(
        ref ProtocolClient receiver,
        ref Attack attack,
        ref GlobalId id
    )
    {
        if (attack.State is { Current: AttackState.Windup, DidStateChange: true })
        {
            receiver.Network.EnqueueOutboundMessage(new AttackWindupNotification(id.Id));
        }

        foreach (var completedAttack in attack.CompletedAttacks)
        {
            ref var entityHealth = ref completedAttack.Entity.TryGetRef<Health>(out var hasHealth);
            ref var targetGid = ref completedAttack.Entity.TryGetRef<GlobalId>(out _);

            if (targetGid.IsPlayer || !hasHealth)
            {
                receiver.Network.EnqueueOutboundMessage(
                    new SlimDamageNotification(targetGid.Id, completedAttack.Damage)
                );
            }
            else
            {
                receiver.Network.EnqueueOutboundMessage(
                    new DamageNotification(targetGid.Id, entityHealth.PercentHealth, id.Id, completedAttack.Damage)
                );
            }
        }
    }

    private static void SendAttackUpdatesFromOthers(
        ref Entity receiverEntity,
        ref ProtocolClient receiver,
        ref Attack attack,
        ref GlobalId id
    )
    {
        if (attack.State is { Current: AttackState.Windup, DidStateChange: true })
        {
            receiver.Network.EnqueueOutboundMessage(new AttackWindupNotification(id.Id));
        }

        foreach (var completedAttack in attack.CompletedAttacks)
        {
            if (receiverEntity != completedAttack.Entity)
            {
                // When sending updates about others' actions, only send updates concerning the receiver.
                continue;
            }

            ref var entityHealth = ref completedAttack.Entity.TryGetRef<Health>(out _);
            ref var targetGid = ref completedAttack.Entity.TryGetRef<GlobalId>(out _);

            receiver.Network.EnqueueOutboundMessage(
                new ExactDamageNotification(
                    targetGid.Id,
                    entityHealth.CurrentHealth,
                    entityHealth.MaxHealth,
                    id.Id,
                    completedAttack.Damage
                )
            );
        }
    }
}
