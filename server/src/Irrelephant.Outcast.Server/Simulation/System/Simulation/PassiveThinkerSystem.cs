using System.Numerics;
using Arch.Core;
using Irrelephant.Outcast.Server.Simulation.Components.Ai;
using Irrelephant.Outcast.Server.Simulation.Components.Behavioral;

namespace Irrelephant.Outcast.Server.Simulation.System.Simulation;

public static class PassiveThinkerSystem
{
    public static void Run(World world)
    {
        var query = new QueryDescription().WithAll<Behavior, PassiveBehavior, Movement>();
        world.Query(
            in query,
            static (ref Behavior behavior, ref Movement movement) =>
            {
                if (behavior.ThinkingCooldownRemaining-- > 0)
                {
                    return;
                }

                behavior.ThinkingCooldownRemaining =
                    behavior.ThinkingCooldown
                    - behavior.ThinkingCooldownVariance
                    + Random.Shared.Next(0, 2 * behavior.ThinkingCooldownVariance);

                Roam(ref behavior, ref movement);
            }
        );

    }

    private static void Roam(ref Behavior behavior, ref Movement movement)
    {
        var roamDistanceFromCenter = Random.Shared.NextSingle() * behavior.RoamDistance;
        var roamAngle = Random.Shared.NextDouble() * Math.Tau;

        movement.TargetPosition = behavior.AnchorPosition + new Vector3(
            roamDistanceFromCenter * (float)Math.Cos(roamAngle),
            0,
            roamDistanceFromCenter * (float)Math.Sin(roamAngle)
        );
    }
}
