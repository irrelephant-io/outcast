using Irrelephant.Outcast.Networking.Protocol;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Encoding;
using Irrelephant.Outcast.Networking.Transport.Abstractions;
using Irrelephant.Outcast.Server.Simulation;
namespace Irrelephant.Outcast.Server.Networking;

public class ProtocolClient(
    ITransportHandler transportHandler,
    IMessageCodec codec
) : DefaultProtocolMessageQueue(transportHandler, codec)
{
    public Guid SessionId = Guid.NewGuid();

    /// <summary>
    /// Shows whether the client for this player has been attached to the simulation engine.
    /// </summary>
    public bool IsAttached { get; set; }

    /// <summary>
    /// Shows whether the client has concluded protocol communications and is ready to be detached from simulation.
    /// </summary>
    public bool IsReadyToDetach { get; set; }

    public void EnsureAttached(OutcastWorld world, ServerNetworkService networkService)
    {
        if (!IsAttached)
        {
            Closed += (_, _) => IsReadyToDetach = true;
            IsAttached = true;
            world.AttachClient(this);
        }
    }

    public void EnsureDetached(OutcastWorld world)
    {
        world.DetachClient(this);
    }
}
