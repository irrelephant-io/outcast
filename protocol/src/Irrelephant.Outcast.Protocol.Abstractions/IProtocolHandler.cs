using Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Messages;

namespace Irrelephant.Outcast.Protocol.Abstractions;

public interface IProtocolHandler
{
    void HandleNewInboundMessage(object? sender, Message message);
}
