using System.Numerics;

namespace Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

public record EntityPositionUpdate(
    Guid EntityId,
    float MovementSpeed,
    Vector3 CurrentPosition,
    float CurrentYAxisRotation
) : Message;
