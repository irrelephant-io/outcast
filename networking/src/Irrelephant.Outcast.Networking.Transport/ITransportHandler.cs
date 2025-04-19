using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer;

namespace Irrelephant.Outcast.Networking.Transport;

public interface ITransportHandler
{
    /// <summary>
    /// Is triggered when a new complete message has been received by the handler.
    /// </summary>
    event EventHandler<TlvMessage> InboundMessage;

    /// <summary>
    /// Is triggered when the underlying transport connection is closed.
    /// </summary>
    event EventHandler Closed;
}
