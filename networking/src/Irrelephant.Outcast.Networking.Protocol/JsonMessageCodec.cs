using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Encoding;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Networking.Transport.Abstractions;

namespace Irrelephant.Outcast.Networking.Protocol;

internal class MessageCodecTypeResolver : DefaultJsonTypeInfoResolver
{
    private JsonTypeInfo? _tlvTypeInfo;

    public readonly IDictionary<Type, int> TlvTypeMap = new Dictionary<Type, int>();

    private JsonTypeInfo CreateTlvTypeInfo(JsonSerializerOptions options)
    {
        _tlvTypeInfo = base.GetTypeInfo(typeof(Message), options);
        _tlvTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions();

        var derivedTypes = typeof(Message).Assembly
            .GetTypes()
            .Where(type => type.IsAssignableTo(typeof(Message)))
            .Where(type => type != typeof(Message))
            .OrderBy(type => type.FullName)
            .Select((type, typeIdx) => new JsonDerivedType(type, typeDiscriminator: typeIdx));

        foreach (var derivedType in derivedTypes)
        {
            TlvTypeMap.Add(derivedType.DerivedType, (int)derivedType.TypeDiscriminator!);
            _tlvTypeInfo.PolymorphismOptions.DerivedTypes.Add(derivedType);
        }

        return _tlvTypeInfo;
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        if (type == typeof(Message))
        {
            return _tlvTypeInfo ??= CreateTlvTypeInfo(options);
        }

        return base.GetTypeInfo(type, options);
    }
}

public class JsonMessageCodec : IMessageCodec
{
    private readonly MessageCodecTypeResolver _typeResolver;
    private readonly JsonSerializerOptions _jsonOptions;
    public JsonMessageCodec()
    {
        _typeResolver = new MessageCodecTypeResolver();
        _jsonOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = _typeResolver
        };
    }

    public Message Decode(TlvMessage tlvMessage)
    {
        return JsonSerializer.Deserialize<Message>(tlvMessage.MessageValue.Span, _jsonOptions)!;
    }

    public TlvMessage Encode(Message message)
    {
        var encodedMessage = JsonSerializer.Serialize(message, _jsonOptions);
        var bytes = System.Text.Encoding.UTF8.GetBytes(encodedMessage);
        return new TlvMessage(
            new TlvHeader(_typeResolver.TlvTypeMap[message.GetType()], MessageLength: bytes.Length),
            bytes.AsMemory()
        );
    }
}
