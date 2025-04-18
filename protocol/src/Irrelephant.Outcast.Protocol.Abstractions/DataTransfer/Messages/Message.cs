using System.Text.Json.Serialization;

namespace Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Messages;

[JsonPolymorphic]
// Heartbeat is handled in a special way since it doesn't contain message body
[JsonDerivedType(typeof(ConnectRequest), typeDiscriminator: 1)]
[JsonDerivedType(typeof(ConnectResponse), typeDiscriminator: 2)]
public abstract record Message
{
    [JsonIgnore]
    internal abstract int TvlType { get; }
}

public record Heartbeat : Message
{
    internal override int TvlType => 0;
}

public record ConnectRequest(string Name) : Message
{
    internal override int TvlType => 1;
};

public record ConnectResponse(string AcceptedName, Guid SessionId) : Message
{
    internal override int TvlType => 2;
}
