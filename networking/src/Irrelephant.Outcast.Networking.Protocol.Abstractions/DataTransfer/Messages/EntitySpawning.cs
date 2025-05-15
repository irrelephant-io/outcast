using System.Numerics;

namespace Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

public record SpawnEntity(
    Guid EntityId,
    Vector3 SpawnPosition,
    float YAxisRotation,
    Guid EntityArchetypeId
) : Message;

public record SpawnPlayerEntity(
    Guid EntityId,
    Vector3 SpawnPosition,
    float YAxisRotation,
    Guid EntityArchetypeId,
    string PlayerName
) : SpawnEntity(EntityId, SpawnPosition, YAxisRotation, EntityArchetypeId);

public record DespawnEntity(
    Guid EntityId
) : Message;
