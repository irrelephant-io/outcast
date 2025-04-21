using System.Numerics;

namespace Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

public abstract record Message;

public record ConnectRequest(string Name) : Message;

public record ConnectResponse(
    string AcceptedName,
    Guid SessionId,
    Guid EntityId,
    Vector3 SpawnPosition,
    float YAxisRotation
) : SpawnEntity(EntityId, SpawnPosition, YAxisRotation);

public record SpawnEntity(Guid EntityId, Vector3 SpawnPosition, float YAxisRotation) : Message;
public record DespawnEntity(Guid EntityId) : Message;
public record InitiateMoveCommand(Vector3 MovePosition) : Message;
public record EntityPositionUpdate(Guid EntityId, Vector3 MovePosition) : Message;
