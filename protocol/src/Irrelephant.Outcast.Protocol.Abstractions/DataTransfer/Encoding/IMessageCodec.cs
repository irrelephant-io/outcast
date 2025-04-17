using Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Messages;

namespace Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Encoding;

public interface IMessageCodec
{
    public Message Decode(TlvMessage tlvMessage);

    public TlvMessage Encode(Message message);
}
