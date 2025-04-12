using Irrelephant.Outcast.Protocol.Messages;

namespace Irrelephant.Outcast.Protocol.Encoding;

public interface IMessageCodec
{
    public Message Decode(TlvMessage tlvMessage);

    public TlvMessage Encode(Message message);
}
