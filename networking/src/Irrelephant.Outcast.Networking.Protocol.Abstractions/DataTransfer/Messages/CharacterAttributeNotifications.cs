namespace Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

public record MaxHealthNotification(
    Guid EntityId,
    int MaxHealth
) : Message;

public record MovementSpeedNotification(
    Guid EntityId,
    float MovementSpeed
) : Message;
