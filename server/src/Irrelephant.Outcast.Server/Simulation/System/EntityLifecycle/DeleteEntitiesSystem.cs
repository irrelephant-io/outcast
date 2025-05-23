﻿using Arch.Core;
using Irrelephant.Outcast.Server.Simulation.Components.Data;

namespace Irrelephant.Outcast.Server.Simulation.System.EntityLifecycle;

public static class DeleteEntitiesSystem
{
    public static void Run(World world)
    {
        world.Destroy(new QueryDescription().WithAll<DespawnMarker>());
    }
}
