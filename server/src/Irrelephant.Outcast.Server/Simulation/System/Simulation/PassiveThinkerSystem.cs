using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Irrelephant.Outcast.Server.Simulation.Components.Ai;
using Irrelephant.Outcast.Server.Simulation.Components.Behavioral;
using Irrelephant.Outcast.Server.Simulation.Components.Data;
using Irrelephant.Outcast.Server.Simulation.Space;

namespace Irrelephant.Outcast.Server.Simulation.System.Simulation;

public static class PassiveThinkerSystem
{
    public static void Run(World world, IPositionTracker positionTracker)
    {
        var query = new QueryDescription().WithAll<Behavior, Movement, Transform, Attack>();
        world.Query(
            in query,
            (ref Behavior behavior, ref Movement movement, ref Transform transform, ref Attack attack) =>
            {
                behavior.State.ClearStateChange();
                while (behavior.State.EnterTick())
                {
                    switch (behavior.State.Current)
                    {
                        case NpcBehaviorState.Roaming: RunRoamingState(ref behavior, ref movement);
                            break;
                        case NpcBehaviorState.Combat: RunCombatState(
                            positionTracker,
                            ref behavior,
                            ref transform,
                            ref attack
                        );
                            break;
                    }
                }
            }
        );
    }

    private static void RunCombatState(
        IPositionTracker positionTracker,
        ref Behavior behavior,
        ref Transform transform,
        ref Attack attack
    )
    {
        if (!behavior.ThreatTable.IsThreatened)
        {
            behavior.State.GoToState(NpcBehaviorState.Roaming);
            attack.AttackTarget = null;
            return;
        }

        if (attack.AttackTarget.HasValue)
        {
            var distanceToAnchorSquared = (transform.Position - behavior.AnchorPosition).LengthSquared();
            if (distanceToAnchorSquared > behavior.PursuitDistance * behavior.PursuitDistance)
            {
                behavior.ThreatTable.DecayThreat(attack.AttackTarget.Value);
            }
        }

        if (behavior.ThinkingCooldownRemaining-- > 0)
        {
            return;
        }
        // In combat, entities think faster.
        behavior.ThinkingCooldownRemaining = behavior.ThinkingCooldown / 2;

        var entitiesInProximity = positionTracker.QueryWithin(transform.Position, 30.0f);
        bool foundTarget = false;
        foreach (var threat in behavior.ThreatTable)
        {
            if (foundTarget)
            {
                behavior.ThreatTable.DecayThreat(threat);
                continue;
            }

            if (threat.IsAlive() && entitiesInProximity.Contains(threat))
            {
                foundTarget = true;
                attack.AttackTarget = threat;
            }
        }
    }

    private static void RunRoamingState(ref Behavior behavior, ref Movement movement)
    {
        if (behavior.ThreatTable.IsThreatened)
        {
            behavior.ThinkingCooldownRemaining = 0;
            behavior.State.GoToState(NpcBehaviorState.Combat);
            return;
        }

        if (behavior.ThinkingCooldownRemaining-- > 0)
        {
            return;
        }

        RefreshThinkCooldown(ref behavior);
        var roamDistanceFromCenter = Random.Shared.NextSingle() * behavior.RoamDistance;
        var roamAngle = Random.Shared.NextDouble() * Math.Tau;

        movement.SetMoveToPosition(
            behavior.AnchorPosition + new Vector3(
                roamDistanceFromCenter * (float)Math.Cos(roamAngle),
                0,
                roamDistanceFromCenter * (float)Math.Sin(roamAngle)
            )
        );
    }

    private static void RefreshThinkCooldown(ref Behavior behavior)
    {
        behavior.ThinkingCooldownRemaining =
            behavior.ThinkingCooldown
            - behavior.ThinkingCooldownVariance
            + Random.Shared.Next(0, 2 * behavior.ThinkingCooldownVariance);
    }
}
