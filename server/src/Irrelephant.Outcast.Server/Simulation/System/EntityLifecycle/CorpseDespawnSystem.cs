using Arch.Core;
using Arch.Core.Extensions;
using Irrelephant.Outcast.Server.Simulation.Components.Data;
using Irrelephant.Outcast.Server.Simulation.Space;

namespace Irrelephant.Outcast.Server.Simulation.System.EntityLifecycle;

public class CorpseDespawnSystem(World world, IPositionTracker positionTracker)
{
    public void Run()
    {
        var query = new QueryDescription().WithAll<Corpse>();
        world.Query(
            in query,
            (Entity entity, ref Corpse corpse) =>
            {
                if (corpse.AutoDespawn)
                {
                    corpse.DespawnTimer--;
                    if (corpse.DespawnTimer <= 0)
                    {
                        entity.Add(new DespawnMarker());
                        positionTracker.Untrack(entity);
                    }
                }
            }
        );
    }
}
