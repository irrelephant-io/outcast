using System.Numerics;

namespace Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

public record SpawnEntity(
    Guid EntityId,
    Vector3 SpawnPosition,
    float YAxisRotation
) : Message;

public record SpawnPlayerEntity(
    Guid EntityId,
    Vector3 SpawnPosition,
    float YAxisRotation,
    string PlayerName
) : SpawnEntity(EntityId, SpawnPosition, YAxisRotation);

public record DespawnEntity(
    Guid EntityId
) : Message;
