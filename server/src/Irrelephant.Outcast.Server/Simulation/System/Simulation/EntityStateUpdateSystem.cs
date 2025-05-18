using Arch.Core;
using Irrelephant.Outcast.Server.Simulation.Components.Data;

namespace Irrelephant.Outcast.Server.Simulation.System.Simulation;

public static class EntityStateUpdateSystem
{
    public static void RunEarlyUpdate(World world)
    {
        var query = new QueryDescription().WithAll<State>();
        world.Query(
            in query,
            static (ref State state) =>
            {
                state.EntityState.ClearStateChange();
            }
        );
    }

    public static void Run(World world)
    {
        var query = new QueryDescription().WithAll<State>();
        world.Query(
            in query,
            static (ref State state) =>
            {
                if (state.EntityState.Current == EntityState.Combat)
                {
                    state.CombatCooldownRemaining--;
                }

                if (state.CombatCooldownRemaining <= 0)
                {
                    state.CombatCooldownRemaining = 0;
                    state.EntityState.GoToState(EntityState.Normal);
                }
            }
        );

    }
}
