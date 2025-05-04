using System.Numerics;

namespace Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

public record ConnectRequest(
    string Name
) : Message;

public record ConnectResponse(
    Guid SessionId,
    Guid EntityId,
    Vector3 SpawnPosition,
    float YAxisRotation
) : SpawnEntity(EntityId, SpawnPosition, YAxisRotation);

public record DisconnectNotice(
    Guid SessionId,
    string Reason
) : Message;
