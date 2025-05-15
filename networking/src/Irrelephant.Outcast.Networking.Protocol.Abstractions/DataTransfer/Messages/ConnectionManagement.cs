using System.Numerics;

namespace Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

public record ConnectRequest(
    string Name
) : Message;

public record ConnectResponse(
    Guid SessionId,
    Guid EntityId,
    Vector3 SpawnPosition,
    float YAxisRotation,
    Guid EntityArchetypeId
) : SpawnEntity(EntityId, SpawnPosition, YAxisRotation, EntityArchetypeId);

public record ConnectTransferComplete(Guid SessionId);

public record DisconnectNotification(
    Guid SessionId,
    string Reason
) : Message;
