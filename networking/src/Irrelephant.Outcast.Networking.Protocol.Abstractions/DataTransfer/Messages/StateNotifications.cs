using System.Numerics;

namespace Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

public record MoveNotification(Guid EntityId, Vector3 MovePosition) : Message;

public record FollowNotification(Guid EntityId, Guid TargetId) : Message;

public record MovementStopNotification(Guid EntityId) : Message;

public record AttackWindupNotification(Guid EntityId) : Message;

public record HealthNotification(Guid EntityId, int RemainingHealth) : Message;

public record DamageNotification(
    Guid DealerId,
    Guid TargetId,
    int Damage,
    Guid? SourceAbilityId
) : Message;

public record EntityPositionNotification(
    Guid EntityId,
    float MovementSpeed,
    Vector3 CurrentPosition,
    float CurrentYAxisRotation
) : Message;

public record CombatStartNotification(Guid EntityId) : Message;

public record CombatEndNotification(Guid EntityId) : Message;

public record IsInCombatNotification(Guid EntityId) : Message;

public record EntityDeathNotification(Guid EntityId) : Message;

public record IsDeadNotification(Guid EntityId) : Message;

public record IsMovingNotification(Guid EntityId) : Message;
