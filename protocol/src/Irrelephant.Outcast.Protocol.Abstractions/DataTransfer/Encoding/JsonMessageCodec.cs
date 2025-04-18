using System.Text.Json;
using Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Messages;

namespace Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Encoding;

public class JsonMessageCodec : IMessageCodec
{
    private readonly JsonSerializerOptions _jsonOptions = new();

    public Message Decode(TlvMessage tlvMessage)
    {
        if (tlvMessage.Header is { MessageLength: 0, MessageType: 0 })
        {
            return new Heartbeat();
        }

        return JsonSerializer.Deserialize<Message>(tlvMessage.MessageValue.Span, _jsonOptions)!;
    }

    public TlvMessage Encode(Message message)
    {
        if (message is Heartbeat)
        {
            return new TlvMessage(
                new TlvHeader(message.TvlType, MessageLength: 0),
                new Memory<byte>()
            );
        }

        var encodedMessage = JsonSerializer.Serialize(message, _jsonOptions);
        var bytes = System.Text.Encoding.UTF8.GetBytes(encodedMessage);
        return new TlvMessage(
            new TlvHeader(message.TvlType, MessageLength: bytes.Length),
            bytes.AsMemory()
        );
    }
}
