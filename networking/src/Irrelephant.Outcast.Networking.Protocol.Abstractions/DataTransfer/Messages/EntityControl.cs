using System.Numerics;

namespace Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

public record InitiateMoveRequest(Vector3 MovePosition) : Message;
public record InitiateFollowRequest(Guid TargetId) : Message;
public record InitiateMoveNotice(Guid EntityId, Vector3 MovePosition) : Message;
public record InitiateFollowNotice(Guid EntityId, Guid TargetId) : Message;
public record MoveDoneNotice(Guid EntityId) : Message;
