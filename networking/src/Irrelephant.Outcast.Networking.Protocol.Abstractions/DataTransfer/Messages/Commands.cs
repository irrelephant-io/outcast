using System.Numerics;

namespace Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

public record MoveCommand(Vector3 MovePosition) : Message;
public record FollowCommand(Guid TargetId) : Message;
public record AttackCommand(Guid EntityId) : Message;
