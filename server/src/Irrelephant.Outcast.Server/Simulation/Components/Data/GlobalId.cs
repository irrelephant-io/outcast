using Irrelephant.Outcast.Server.Simulation.System.EntityLifecycle;

namespace Irrelephant.Outcast.Server.Simulation.Components.Data;

public struct GlobalId : IComponent
{
    public Guid Id;
    public Guid ArchetypeId;

    public bool IsPlayer => ArchetypeId == ArchetypeRegistry.PlayerArchetypeId;
}
