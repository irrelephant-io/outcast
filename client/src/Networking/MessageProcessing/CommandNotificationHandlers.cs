using Irrelephant.Outcast.Client.Entities;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

namespace Irrelephant.Outcast.Client.Networking.MessageProcessing;

public class MoveNotificationHandler : IMessageHandler<MoveNotification>
{
    public void Process(MoveNotification message)
    {
        NetworkedEntity.GetByRemoteId(message.EntityId)?.NotifyMovementStart();
    }
}

public class FollowNotificationHandler : IMessageHandler<FollowNotification>
{
    public void Process(FollowNotification message)
    {
        NetworkedEntity.GetByRemoteId(message.EntityId)?.NotifyMovementStart();
    }
}

public class MovementStopNotificationHandler : IMessageHandler<MovementStopNotification>
{
    public void Process(MovementStopNotification message)
    {
        NetworkedEntity.GetByRemoteId(message.EntityId)?.NotifyMovementEnd();
    }
}


public class AttackWindupNotificationHandler : IMessageHandler<AttackWindupNotification>
{
    public void Process(AttackWindupNotification message)
    {
        NetworkedEntity.GetByRemoteId(message.EntityId)?.NotifyAttack();
    }
}
