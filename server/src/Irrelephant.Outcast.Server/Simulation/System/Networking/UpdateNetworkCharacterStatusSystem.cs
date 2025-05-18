using System.Diagnostics;
using Arch.Core;
using Arch.Core.Extensions;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Server.Simulation.Components.Behavioral;
using Irrelephant.Outcast.Server.Simulation.Components.Communication;
using Irrelephant.Outcast.Server.Simulation.Components.Data;
using Irrelephant.Outcast.Server.Simulation.Extensions;

namespace Irrelephant.Outcast.Server.Simulation.System.Networking;

public static class UpdateNetworkCharacterStatusSystem
{
    public static void Run(World world)
    {
        var allClients = new QueryDescription()
            .WithAll<ProtocolClient, GlobalId, Transform>()
            .WithAny<Movement, Attack, Health, State>();

        world.Query(
            in allClients,
            (Entity entity, ref ProtocolClient pc, ref GlobalId id, ref Transform transform) =>
            {
                ref var attack = ref entity.TryGetRef<Attack>(out var hasAttack);
                ref var movement = ref entity.TryGetRef<Movement>(out var hasMovement);
                ref var state = ref entity.TryGetRef<State>(out var hasState);
                ref var health = ref entity.TryGetRef<Health>(out var hasHealth);

                if (hasMovement)
                {
                    SendMoveUpdate(ref pc, ref movement, ref id, ref transform);
                }

                if (hasAttack)
                {
                    SendOwnAttackUpdates(ref pc, ref attack, ref id);
                }

                if (hasState)
                {
                    SendStateUpdates(ref pc, ref state, ref id);
                }

                if (hasHealth)
                {
                    SendHealthUpdates(ref pc, ref health, ref id);
                }

                foreach (var entityWithin in pc.InterestSphere.EntitiesWithin)
                {
                    ref var entityWithinGid = ref entityWithin.TryGetRef<GlobalId>(out _);
                    ref var interestSphereMovement = ref entityWithin.TryGetRef<Movement>(
                        out var interestSphereHasMovement
                    );
                    if (interestSphereHasMovement)
                    {
                        var entityWithinTransform = entityWithin.Get<Transform>();
                        SendMoveUpdate(
                            ref pc,
                            ref interestSphereMovement,
                            ref entityWithinGid,
                            ref entityWithinTransform
                        );
                    }

                    ref var interestSphereAttack = ref entityWithin.TryGetRef<Attack>(
                        out var interestSphereHasAttack
                    );
                    if (interestSphereHasAttack)
                    {
                        SendAttackUpdatesFromOthers(
                            ref entity,
                            ref pc,
                            ref interestSphereAttack,
                            ref entityWithinGid
                        );
                    }

                    ref var interestSphereState = ref entityWithin.TryGetRef<State>(
                        out var interestSphereHasState
                    );
                    if (interestSphereHasState)
                    {
                        SendStateUpdates(ref pc, ref interestSphereState, ref entityWithinGid);
                    }

                    if (!entityWithinGid.IsPlayer)
                    {
                        ref var interestSphereHealth = ref entityWithin.TryGetRef<Health>(out var interestSphereHasHealth);
                        if (interestSphereHasHealth)
                        {
                            SendHealthUpdates(ref pc, ref interestSphereHealth, ref entityWithinGid);
                        }
                    }
                }
            }
        );
    }

    private static void SendHealthUpdates(
        ref ProtocolClient protocolClient,
        ref Health health,
        ref GlobalId gid
    )
    {
        if (health.HealthChangedThisTick)
        {
            protocolClient.Network.EnqueueOutboundMessage(new HealthNotification(gid.Id, health.CurrentHealth));
        }
    }

    private static void SendStateUpdates(
        ref ProtocolClient protocolClient,
        ref State state,
        ref GlobalId gid
    )
    {
        if (state.EntityState.DidStateChange)
        {
            Message message = state.EntityState.Current switch
            {
                EntityState.Combat => new CombatStartNotification(gid.Id),
                EntityState.Normal => new CombatEndNotification(gid.Id),
                EntityState.Dead => new EntityDeathNotification(gid.Id),
                _ => throw new UnreachableException()
            };
            protocolClient.Network.EnqueueOutboundMessage(message);
        }
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
            ref var targetGid = ref completedAttack.Entity.TryGetRef<GlobalId>(out _);
            receiver.Network.EnqueueOutboundMessage(
                new DamageNotification(id.Id, targetGid.Id, completedAttack.Damage, SourceAbilityId: null)
            );
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

            ref var targetGid = ref completedAttack.Entity.TryGetRef<GlobalId>(out _);

            receiver.Network.EnqueueOutboundMessage(
                new DamageNotification(id.Id, targetGid.Id, completedAttack.Damage, null)
            );
        }
    }
}
