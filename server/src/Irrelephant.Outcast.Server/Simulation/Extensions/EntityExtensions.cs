using Arch.Core;
using Arch.Core.Extensions;
using Irrelephant.Outcast.Server.Simulation.Components.Data;

namespace Irrelephant.Outcast.Server.Simulation.Extensions;

public static class EntityExtensions
{
    public static bool IsPlayer(this ref Entity entity)
    {
        ref var gid = ref entity.TryGetRef<GlobalId>(out var gidExists);
        return gidExists && gid.IsPlayer;
    }
}
