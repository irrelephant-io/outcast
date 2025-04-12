using System.Text.Json;
using Irrelephant.Outcast.Protocol.Messages;

namespace Irrelephant.Outcast.Protocol.Encoding;

public class JsonMessageCodec : IMessageCodec
{
    private readonly JsonSerializerOptions _jsonOptions = new();


    public Message Decode(TlvMessage tlvMessage)
    {
        return JsonSerializer.Deserialize<Message>(tlvMessage.MessageValue.Span, _jsonOptions)!;
    }

    public TlvMessage Encode(Message message)
    {
        var encodedMessage = JsonSerializer.Serialize(message, _jsonOptions);
        var bytes = System.Text.Encoding.UTF8.GetBytes(encodedMessage);
        return new TlvMessage(
            new TlvHeader(message.TvlType, MessageLength: bytes.Length),
            bytes.AsMemory()
        );
    }
}
