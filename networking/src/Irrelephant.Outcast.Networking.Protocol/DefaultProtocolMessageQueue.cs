using System.Diagnostics.CodeAnalysis;
using Irrelephant.Outcast.Networking.Protocol.Abstractions;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Encoding;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Networking.Transport.Abstractions;

namespace Irrelephant.Outcast.Networking.Protocol;

/// <inheritdoc/>
public class DefaultProtocolMessageQueue : IProtocolMessageQueue
{
    private readonly ITransportHandler _transportHandler;
    private readonly IMessageCodec _codec;

    public DefaultProtocolMessageQueue(
        ITransportHandler transportHandler,
        IMessageCodec messageCodec)
    {
        _transportHandler = transportHandler;
        _codec = messageCodec;
        _transportHandler.Closed += (_, _) =>
        {
            Closed?.Invoke(this, EventArgs.Empty);
        };
    }

    public event EventHandler? Closed;

    /// <inheritdoc/>
    public bool TryDequeueInboundMessage([NotNullWhen(true)] out Message? inboundMessage)
    {
        if (_transportHandler.InboundMessages.TryDequeue(out var inboundTlv))
        {
            inboundMessage = _codec.Decode(inboundTlv);
            return true;
        }

        inboundMessage = null;
        return false;
    }

    /// <inheritdoc/>
    public void EnqueueOutboundMessage(Message message) =>
        _transportHandler.EnqueueOutboundMessage(
            _codec.Encode(message)
        );

    /// <inheritdoc/>
    public void Receive() =>
        _transportHandler.Receive();

    /// <inheritdoc/>
    public void Transmit() =>
        _transportHandler.Transmit();

    /// <inheritdoc/>
    public DateTime LastNetworkActivity => _transportHandler.LastNetworkActivity;

    public void Dispose()
    {
        _transportHandler.Dispose();
    }

    /// <inheritdoc/>
    public void EnqueueHeartbeatMessage()
    {
        _transportHandler.EnqueueOutboundMessage(
            new TlvMessage(
                Header: new TlvHeader(0, 0),
                MessageValue: Memory<byte>.Empty
            )
        );
    }
}
