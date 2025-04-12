using System.Text.Json.Serialization;

namespace Irrelephant.Outcast.Protocol.DataTransfer.Messages;

[JsonPolymorphic]
[JsonDerivedType(typeof(Heartbeat), typeDiscriminator: 0)]
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
