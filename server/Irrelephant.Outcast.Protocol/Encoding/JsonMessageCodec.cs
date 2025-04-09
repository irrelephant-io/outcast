using System.Text.Json;
using Irrelephant.Outcast.Protocol.Messages;

namespace Irrelephant.Outcast.Protocol.Encoding;

public class JsonMessageCodec : IMessageCodec
{
    public Message Decode(TlvMessage tlvMessage)
    {
        return JsonSerializer.Deserialize<Message>(tlvMessage.MessageValue.Span)!;
    }

    public TlvMessage Encode(Message message)
    {
        var encodedMessage = JsonSerializer.Serialize(message);
        var bytes = System.Text.Encoding.UTF8.GetBytes(encodedMessage);
        return new TlvMessage(
            new TlvHeader(0, MessageLength: bytes.Length),
            bytes.AsMemory()
        );
    }
}
