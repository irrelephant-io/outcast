using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Networking.Transport.EventModel;

namespace Irrelephant.Outcast.Networking.Transport;

public interface IMessageHandler
{
    public DateTimeOffset LastNetworkActivity { get; }

    event EventHandler<Message>? InboundMessageReceived;

    event EventHandler<OutboundMessageAvailableArgs>? OutboundMessageAvailable;

    void ProcessRead(Memory<byte> buffer, int receivedBytes, bool isAsync = true);
    void EnqueueMessage(Message message);
    void Reset();
}
