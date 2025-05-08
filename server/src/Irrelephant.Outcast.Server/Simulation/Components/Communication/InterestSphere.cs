namespace Irrelephant.Outcast.Server.Simulation.Components.Communication;

public class InterestSphere
{
    public readonly float Radius = 100.0f;
    public ISet<Arch.Core.Entity> EntitiesWithin { get; set; } = new HashSet<Arch.Core.Entity>();
    public ISet<Arch.Core.Entity> LeavingEntities { get; set; } = new HashSet<Arch.Core.Entity>();
    public ISet<Arch.Core.Entity> EnteringEntities { get; set; } = new HashSet<Arch.Core.Entity>();
}
