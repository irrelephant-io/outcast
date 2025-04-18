using Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Messages;

namespace Irrelephant.Outcast.Protocol.Networking.Session;

public interface IProtocolHandler
{
    void HandleNewInboundMessage(object? sender, Message message);
    void HandleAfterTransportConnected();
    void HandleBeforeTransportDisconnected();
}
