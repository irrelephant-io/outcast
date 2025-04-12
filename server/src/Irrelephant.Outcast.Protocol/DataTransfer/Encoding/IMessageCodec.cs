using Irrelephant.Outcast.Protocol.DataTransfer.Messages;

namespace Irrelephant.Outcast.Protocol.DataTransfer.Encoding;

public interface IMessageCodec
{
    public Message Decode(TlvMessage tlvMessage);

    public TlvMessage Encode(Message message);
}
