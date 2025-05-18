using Godot;
using Irrelephant.Outcast.Client.Entities;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

namespace Irrelephant.Outcast.Client.Networking.MessageProcessing;

public class MovementSpeedNotificationHandler : IMessageHandler<MovementSpeedNotification>
{
    public void Process(MovementSpeedNotification message)
    {
        // No-op for now
    }
}

public class MaxHealthNotificationHandler : IMessageHandler<MaxHealthNotification>
{
    public void Process(MaxHealthNotification message)
    {
        Callable.From(() =>
        {
            var entity = NetworkedEntity.GetByRemoteId(message.EntityId);
            entity.MaxHealth = message.MaxHealth;
        }).CallDeferred();
    }
}
