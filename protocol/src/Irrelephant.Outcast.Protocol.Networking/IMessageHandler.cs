using Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Protocol.Networking.EventModel;

namespace Irrelephant.Outcast.Protocol.Networking;

public interface IMessageHandler
{
    public DateTimeOffset LastNetworkActivity { get; }

    event EventHandler<Message>? InboundMessageReceived;

    event EventHandler<OutboundMessageAvailableArgs>? OutboundMessageAvailable;

    void ProcessRead(Memory<byte> buffer, int receivedBytes, bool isAsync = true);
    void EnqueueMessage(Message message);
    void Reset();
}
