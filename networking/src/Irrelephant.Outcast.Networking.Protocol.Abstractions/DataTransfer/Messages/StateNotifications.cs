using System.Numerics;

namespace Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

public record MoveNotification(Guid EntityId, Vector3 MovePosition) : Message;

public record FollowNotification(Guid EntityId, Guid TargetId) : Message;

public record MovementStopNotification(Guid EntityId) : Message;

public record AttackWindupNotification(Guid EntityId) : Message;

public record DamageNotification(
    Guid EntityId,
    int PercentHealth,
    Guid DamageDealerId,
    int Damage
) : HealthNotification(EntityId, PercentHealth);

public record HealthNotification(Guid EntityId, int PercentHealth) : Message;

public record SlimDamageNotification(Guid EntityId, int Damage) : Message;

public record ExactDamageNotification(
    Guid EntityId,
    int CurrentHealth,
    int MaximumHealth,
    Guid DamageDealerId,
    int Damage
) : ExactHealthNotification(EntityId, CurrentHealth, MaximumHealth);

public record ExactHealthNotification(Guid EntityId, int CurrentHealth, int MaximumHealth) : Message;

public record EntityPositionNotification(
    Guid EntityId,
    float MovementSpeed,
    Vector3 CurrentPosition,
    float CurrentYAxisRotation
) : Message;

public record CombatStartNotification(Guid EntityId) : Message;

public record CombatEndNotification(Guid EntityId) : Message;

public record EntityDeathNotification(Guid EntityId) : Message;
