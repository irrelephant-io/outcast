using Arch.Core;

namespace Irrelephant.Outcast.Server.Simulation.Components;

public class InterestSphere
{
    public readonly float Radius = 20.0f;
    public ISet<Entity> EntitiesWithin { get; set; } = new HashSet<Entity>();
    public ISet<Entity> LeavingEntities { get; set; } = new HashSet<Entity>();
    public ISet<Entity> EnteringEntities { get; set; } = new HashSet<Entity>();
}

public struct ProtocolClient(Networking.ProtocolClient protocolClient)
{
    public readonly Networking.ProtocolClient Network = protocolClient;

    public readonly InterestSphere InterestSphere = new();
}
