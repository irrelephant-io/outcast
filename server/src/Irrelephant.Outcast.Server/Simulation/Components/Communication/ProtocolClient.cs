namespace Irrelephant.Outcast.Server.Simulation.Components.Communication;

public struct ProtocolClient(Networking.ProtocolClient protocolClient)
{
    public readonly Networking.ProtocolClient Network = protocolClient;

    public readonly InterestSphere InterestSphere = new();
}
