using Arch.Buffer;
using Arch.Core;
using Irrelephant.Outcast.Server.Simulation.Components.Ai;
using Irrelephant.Outcast.Server.Simulation.Components.Behavioral;
using Irrelephant.Outcast.Server.Simulation.Components.Data;
using Irrelephant.Outcast.Server.Simulation.Extensions;

namespace Irrelephant.Outcast.Server.Simulation.System.EntityLifecycle;

public static class EntityHealthUpdateSystem
{
    public static void RunEarlyUpdate(World world)
    {
        var query = new QueryDescription().WithAll<Health, State>();
        world.Query(
            in query,
            static (ref Health health, ref State state) =>
            {
                health.HealthChangedThisTick = false;
                if (health.IsAlive())
                {
                    if (state.EntityState.Current == EntityState.Normal)
                    {
                        health.RegenerationCooldownRemaining--;
                        if (health.RegenerationCooldownRemaining <= 0)
                        {
                            health.AddHealth(health.HealthRecoveryPerRegenerationTick);
                            health.RegenerationCooldownRemaining = health.RegenerationCooldown;
                        }
                    }
                }
            }
        );
    }

    public static void RunLateUpdate(World world)
    {
        var commandBuffer = new CommandBuffer();
        var query = new QueryDescription().WithAll<Health, State>();
        world.Query(
            in query,
            (Entity entity, ref Health health, ref State state) =>
            {
                if (!health.IsAlive())
                {
                    var isPlayer = entity.IsPlayer();
                    commandBuffer.Add(entity, new Corpse
                    {
                        AutoDespawn = !isPlayer,
                        DespawnTimer = isPlayer ? -1 : 200
                    });
                    commandBuffer.Remove<Attack>(entity);
                    commandBuffer.Remove<Movement>(entity);
                    commandBuffer.Remove<Behavior>(entity);
                    commandBuffer.Remove<PassiveBehavior>(entity);
                    commandBuffer.Remove<AggressiveBehavior>(entity);
                    state.EntityState.GoToState(EntityState.Dead);
                }
            }
        );

        commandBuffer.Playback(world);
    }

    public static void RunVeryLateUpdate(World world)
    {
        // Health component is removed from dead entities at the very end because network information
        // about the health change needs to be flushed to the clients.
        var query = new QueryDescription().WithAll<Health>();
        var commandBuffer = new CommandBuffer();
        world.Query(
            in query,
            (Entity entity, ref Health health) =>
            {
                if (!health.IsAlive())
                {
                    commandBuffer.Remove<Health>(entity);
                }
            }
        );
        commandBuffer.Playback(world);
    }
}
