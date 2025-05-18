using Godot;
using Irrelephant.Outcast.Client.Entities;
using Irrelephant.Outcast.Client.Networking.Extensions;
using Irrelephant.Outcast.Client.Ui.Control;
using Irrelephant.Outcast.Client.Ui.Interface.UiState.Gameplay;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

namespace Irrelephant.Outcast.Client.Networking.MessageProcessing;

public class EntityPositionNotificationHandler : IMessageHandler<EntityPositionNotification>
{
    public void Process(EntityPositionNotification message)
    {
        if (NetworkedEntity.GetByRemoteId(message.EntityId) is {} entity)
        {
            entity.SetServerPositionalData(
                message.CurrentPosition.ToGodotVector(),
                message.CurrentYAxisRotation
            );
        }
    }
}

public class DamageNotificationHandler : IMessageHandler<DamageNotification>
{
    public void Process(DamageNotification message)
    {
        var currentPlayer = PlayerController.Instance.ControlledPlayer;
        var dealer = NetworkedEntity.GetByRemoteId(message.DealerId);
        var dealerName = dealer == currentPlayer ? "You" : dealer.EntityName;

        var receiver = NetworkedEntity.GetByRemoteId(message.TargetId);
        var receiverName = receiver == currentPlayer ? "you" : receiver.EntityName;
        SystemConsole.Instance?.AddMessage(
            $"{dealerName} dealt [b][color=red]{message.Damage}[/color][/b] damage to {receiverName}."
        );
    }
}

public class HealthNotificationHandler : IMessageHandler<HealthNotification>
{
    public void Process(HealthNotification message)
    {
        Callable.From(
            () => NetworkedEntity.GetByRemoteId(message.EntityId)?.SetServerHealthData(message.RemainingHealth)
        )
        .CallDeferred();
    }
}

public class CombatStartNotificationHandler : IMessageHandler<CombatStartNotification>
{
    public void Process(CombatStartNotification message)
    {
        if (message.EntityId == PlayerController.Instance.ControlledPlayer?.RemoteId)
        {
            SystemConsole.Instance?.AddMessage(
                "[color=yellow]You are now in combat![/color]"
            );
        }

        NetworkedEntity.GetByRemoteId(message.EntityId)!.NotifyEnterCombat();
    }
}

public class CombatEndNotificationHandler : IMessageHandler<CombatEndNotification>
{
    public void Process(CombatEndNotification message)
    {
        if (message.EntityId == PlayerController.Instance.ControlledPlayer?.RemoteId)
        {
            SystemConsole.Instance?.AddMessage(
                "[color=yellow]You are no longer in combat![/color]"
            );
        }

        NetworkedEntity.GetByRemoteId(message.EntityId)!.NotifyLeaveCombat();
    }
}

public class EntityDeathNotificationHandler : IMessageHandler<EntityDeathNotification>
{
    public void Process(EntityDeathNotification message)
    {
        var dyingEntity = NetworkedEntity.GetByRemoteId(message.EntityId);
        if (message.EntityId == PlayerController.Instance.ControlledPlayer?.RemoteId)
        {
            SystemConsole.Instance?.AddMessage("[color=red]Oh dear... You are dead![/color]");
        }
        else
        {
            var dyingEntityName = dyingEntity.EntityName;
            SystemConsole.Instance?.AddMessage($"[color=yellow]{dyingEntityName} dies![/color]");
        }
        dyingEntity.NotifyDeath();
    }
}
