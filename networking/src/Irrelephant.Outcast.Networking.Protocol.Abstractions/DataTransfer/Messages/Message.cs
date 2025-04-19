using System.Text.Json.Serialization;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages.Primitives;

namespace Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

[JsonPolymorphic]
// Heartbeat is handled in a special way since it doesn't contain message body
[JsonDerivedType(typeof(ConnectRequest), typeDiscriminator: 1)]
[JsonDerivedType(typeof(ConnectResponse), typeDiscriminator: 2)]
[JsonDerivedType(typeof(SpawnEntity), typeDiscriminator: 3)]
[JsonDerivedType(typeof(DespawnEntity), typeDiscriminator: 4)]
[JsonDerivedType(typeof(MoveEntity), typeDiscriminator: 5)]
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

public record ConnectResponse(
    string AcceptedName,
    Guid SessionId,
    Guid EntityId,
    Vector3Primitive SpawnPosition,
    float YAxisRotation
) : SpawnEntity(EntityId, SpawnPosition, YAxisRotation)
{
    internal override int TvlType => 2;
}

public record SpawnEntity(
    Guid EntityId,
    Vector3Primitive SpawnPosition,
    float YAxisRotation
) : Message
{
    internal override int TvlType => 3;
};

public record DespawnEntity(
    Vector3Primitive SpawnPosition,
    float YAxisRotation
) : Message
{
    internal override int TvlType => 4;
};

public record MoveEntity(
    Guid EntityId,
    Vector3Primitive MovePosition
) : Message
{
    internal override int TvlType => 5;
}
