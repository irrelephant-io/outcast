using Godot;
using Irrelephant.Outcast.Client.Entities;
using Irrelephant.Outcast.Client.Networking.Extensions;
using Irrelephant.Outcast.Client.Ui.Interface;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

namespace Irrelephant.Outcast.Client.Networking.MessageProcessing;

public class SpawnPlayerEntityHandler : IMessageHandler<SpawnPlayerEntity>
{
    public void Process(SpawnPlayerEntity message)
    {
        var player = NetworkedEntity.Spawn<PlayerEntity>(
            message.EntityId,
            message.SpawnPosition.ToGodotVector(),
            message.YAxisRotation
        );
        player.SetServerPositionalData(
            message.SpawnPosition.ToGodotVector(),
            message.YAxisRotation
        );
        player.SetEntityName(message.PlayerName);
    }
}

public class SpawnEntityHandler : IMessageHandler<SpawnEntity>
{
    public void Process(SpawnEntity message)
    {
        var entity = NetworkedEntity.Spawn<NpcEntity>(
            message.EntityId,
            message.SpawnPosition.ToGodotVector(),
            message.YAxisRotation
        );
        entity.SetServerPositionalData(
            message.SpawnPosition.ToGodotVector(),
            message.YAxisRotation
        );
        entity.SetEntityName("Some Random Mob");
    }
}

public class DespawnEntityHandler : IMessageHandler<DespawnEntity>
{
    public void Process(DespawnEntity message)
    {
        var despawningEntity = NetworkedEntity.GetByRemoteId(message.EntityId);
        if (UiController.Instance.TargetedEntity == despawningEntity)
        {
            UiController.Instance.SetTarget(null);
        }
        despawningEntity.QueueFree();
    }
}

public class IsDeadNotificationHandler : IMessageHandler<IsDeadNotification>
{
    public void Process(IsDeadNotification message)
    {
        Callable.From(() => NetworkedEntity.GetByRemoteId(message.EntityId).NotifyDead()).CallDeferred();
    }
}

public class IsInCombatNotificationHandler : IMessageHandler<IsInCombatNotification>
{
    public void Process(IsInCombatNotification message)
    {
        Callable.From(() => NetworkedEntity.GetByRemoteId(message.EntityId).NotifyEnterCombat()).CallDeferred();
    }
}

public class IsMovingNotificationHandler : IMessageHandler<IsMovingNotification>
{
    public void Process(IsMovingNotification message)
    {
        Callable.From(() => NetworkedEntity.GetByRemoteId(message.EntityId).NotifyMovementStart()).CallDeferred();
    }
}
