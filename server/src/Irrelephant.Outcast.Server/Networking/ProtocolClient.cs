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

    public void EnsureAttached(OutcastWorld world)
    {
        if (!IsAttached)
        {
            IsAttached = true;
            world.AttachClient(this);
        }
    }
}
