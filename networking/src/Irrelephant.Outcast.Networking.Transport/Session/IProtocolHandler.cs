using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

namespace Irrelephant.Outcast.Networking.Transport.Session;

public interface IProtocolHandler
{
    void HandleNewInboundMessage(object? sender, Message message);
    void HandleAfterTransportConnected();
    void HandleBeforeTransportDisconnected();
}
