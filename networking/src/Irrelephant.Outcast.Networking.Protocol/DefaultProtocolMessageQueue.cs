using System.Diagnostics.CodeAnalysis;
using Irrelephant.Outcast.Networking.Protocol.Abstractions;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Encoding;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Networking.Transport.Abstractions;

namespace Irrelephant.Outcast.Networking.Protocol;

/// <inheritdoc/>
public class DefaultProtocolMessageQueue(
    ITransportHandler transportHandler,
    IMessageCodec codec
) : IProtocolMessageQueue
{
    /// <inheritdoc/>
    public bool TryDequeueInboundMessage([NotNullWhen(true)] out Message? inboundMessage)
    {
        if (transportHandler.InboundMessages.TryDequeue(out var inboundTlv))
        {
            inboundMessage = codec.Decode(inboundTlv);
            return true;
        }

        inboundMessage = null;
        return false;
    }

    /// <inheritdoc/>
    public void EnqueueOutboundMessage(Message message) =>
        transportHandler.EnqueueOutboundMessage(
            codec.Encode(message)
        );

    /// <inheritdoc/>
    public void Receive() =>
        transportHandler.Receive();

    /// <inheritdoc/>
    public void Transmit() =>
        transportHandler.Transmit();

    public void Dispose()
    {
        transportHandler.Dispose();
    }
}
