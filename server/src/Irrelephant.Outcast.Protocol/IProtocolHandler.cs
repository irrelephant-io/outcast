using Irrelephant.Outcast.Protocol.DataTransfer.Messages;

namespace Irrelephant.Outcast.Protocol;

public interface IProtocolHandler
{
    void HandleNewInboundMessage(object? sender, Message message);
}
