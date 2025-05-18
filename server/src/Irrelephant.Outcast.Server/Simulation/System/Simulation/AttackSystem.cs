using System.Diagnostics;
using Arch.Core;
using Arch.Core.Extensions;
using Irrelephant.Outcast.Server.Simulation.Components.Ai;
using Irrelephant.Outcast.Server.Simulation.Components.Behavioral;
using Irrelephant.Outcast.Server.Simulation.Components.Data;

namespace Irrelephant.Outcast.Server.Simulation.System.Simulation;

public static class AttackSystem
{
    public static void Run(World world)
    {
        var allAttackersQuery = new QueryDescription().WithAll<Attack, State, Transform>();
        world.Query(
            in allAttackersQuery,
            static (Entity entity, ref Attack attack, ref State state, ref Transform transform) =>
            {
                attack.ClearCompletedAttacks();
                attack.State.ClearStateChange();

                while (attack.State.EnterTick())
                {
                    switch (attack.State.Current)
                    {
                        case AttackState.Ready:
                            RunReadyState(ref entity, ref attack, ref state, ref transform);
                            break;
                        case AttackState.Windup:
                            RunWindupState(ref attack);
                            break;
                        case AttackState.Damage:
                            RunDamageState(entity, ref attack);
                            break;
                        case AttackState.Recovery:
                            RunRecoveryState(ref entity, ref attack);
                            break;
                        default:
                            throw new UnreachableException();
                    }
                }
            }
        );
    }

    private static void RunReadyState(ref Entity attacker, ref Attack attack, ref State state, ref Transform transform)
    {
        if (!attack.AttackTarget.HasValue)
        {
            return;
        }

        if (!attack.AttackTarget.Value.IsAlive())
        {
            attack.ClearAttackCommand();
            return;
        }

        ref var targetTransform = ref attack.AttackTarget.Value.TryGetRef<Transform>(out var targetExists);
        if (targetExists)
        {
            var isInRange = (transform.Position - targetTransform.Position).LengthSquared()
                            <= attack.Range * attack.Range;

            ref var attackerMovable = ref attacker.TryGetRef<Movement>(out var isMovable);
            if (isInRange)
            {
                attack.State.GoToState(AttackState.Windup);
                attack.AttackCooldownRemaining = attack.AttackCooldown;
                attack.LockedInTarget = attack.AttackTarget;
                state.EnterCombat();
                if (isMovable)
                {
                    attackerMovable.LockMovement();
                }
            }
            else
            {
                if (isMovable)
                {
                    attackerMovable.SetFollowEntity(attack.AttackTarget.Value, attack.Range);
                }
            }
        }
    }

    private static void RunWindupState(ref Attack attack) {
        attack.AttackCooldownRemaining--;

        // For now, let's make wind up take 30% of the attack timer + 1 tick.
        if (attack.AttackCooldownRemaining < attack.AttackCooldown * 0.7f)
        {
            attack.State.GoToState(AttackState.Damage);
        }
    }

    private static void RunDamageState(Entity entity, ref Attack attack)
    {
        if (attack.LockedInTarget!.Value.IsAlive())
        {
            ref var targetHealth = ref attack.LockedInTarget!.Value.TryGetRef<Health>(out var hasHealth);
            if (hasHealth)
            {
                targetHealth.CurrentHealth -= attack.Damage;
                if (!targetHealth.IsAlive())
                {
                    attack.ClearAttackCommand();
                }
            }

            ref var targetBehavior = ref attack.LockedInTarget!.Value.TryGetRef<Behavior>(out var hasBehavior);
            if (hasBehavior)
            {
                targetBehavior.ThreatTable.AddThreat(entity, attack.Damage);
            }

            ref var targetState = ref attack.LockedInTarget!.Value.TryGetRef<State>(out var hasState);
            if (hasState)
            {
                targetState.EnterCombat();
            }

            attack.CompletedAttacks.Add(
                new AttackDamageDealt {
                    Entity = attack.LockedInTarget!.Value,
                    Damage = attack.Damage
                }
            );
        }

        attack.State.GoToState(AttackState.Recovery);
    }

    private static void RunRecoveryState(ref Entity attacker, ref Attack attack)
    {
        attack.AttackCooldownRemaining--;
        if (attack.AttackCooldownRemaining == 0)
        {
            attack.State.GoToState(AttackState.Ready);
            ref var attackerMovable = ref attacker.TryGetRef<Movement>(out var isMovable);
            if (isMovable)
            {
                attackerMovable.UnlockMovement();
            }

            attack.LockedInTarget = null;
        }
    }
}
