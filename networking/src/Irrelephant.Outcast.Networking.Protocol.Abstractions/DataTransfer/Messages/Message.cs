using System.Text.Json.Serialization;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages.Primitives;

namespace Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

[JsonDerivedType(typeof(ConnectRequest), typeDiscriminator: 1)]
[JsonDerivedType(typeof(ConnectResponse), typeDiscriminator: 2)]
public abstract record Message;

public record ConnectRequest(string Name) : Message;

public record ConnectResponse(
    string AcceptedName,
    Guid SessionId,
    Guid EntityId,
    Vector3Primitive SpawnPosition,
    float YAxisRotation
) : SpawnEntity(EntityId, SpawnPosition, YAxisRotation);

public record SpawnEntity(
    Guid EntityId,
    Vector3Primitive SpawnPosition,
    float YAxisRotation
) : Message;

public record DespawnEntity(
    Vector3Primitive SpawnPosition,
    float YAxisRotation
) : Message;

public record MoveEntity(
    Guid EntityId,
    Vector3Primitive MovePosition
) : Message;
