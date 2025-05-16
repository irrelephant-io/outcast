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

public class SlimDamageNotificationHandler : IMessageHandler<SlimDamageNotification>
{
    public void Process(SlimDamageNotification message)
    {
        // Slim damage notification may only come in response to PvP combat and
        // must be initiated by the current player.
        var dealer = PlayerController.Instance.ControlledPlayer!.EntityName;
        var receiver = NetworkedEntity.GetByRemoteId(message.EntityId)?.EntityName ?? "<unknown>";
        SystemConsole.Instance?.AddMessage(
            $"{dealer} dealt [b][color=red]{message.Damage}[/color][/b] to {receiver}."
        );
    }
}

public class DamageNotificationHandler : IMessageHandler<DamageNotification>
{
    public void Process(DamageNotification message)
    {
        var dealer = PlayerController.Instance.ControlledPlayer!.EntityName;
        var receiverEntity = NetworkedEntity.GetByRemoteId(message.EntityId);
        var receiver = receiverEntity?.EntityName ?? "<unknown>";
        SystemConsole.Instance?.AddMessage(
            $"{dealer} dealt [b][color=red]{message.Damage}[/color][/b] to {receiver}."
        );
        receiverEntity?.SetServerHealthData(message.PercentHealth);
    }
}

public class ExactDamageNotificationHandler : IMessageHandler<ExactDamageNotification>
{
    public void Process(ExactDamageNotification message)
    {
        var dealer = NetworkedEntity.GetByRemoteId(message.DamageDealerId)?.EntityName ?? "<unknown>";
        var receiverEntity = NetworkedEntity.GetByRemoteId(message.EntityId);
        var receiver = receiverEntity?.EntityName ?? "<unknown>";

        SystemConsole.Instance?.AddMessage(
            $"{dealer} dealt [b][color=red]{message.Damage}[/color][/b] to {receiver}."
        );

        receiverEntity?.SetServerHealthData(message.CurrentHealth / message.MaximumHealth);
    }
}

public class HealthNotificationHandler : IMessageHandler<HealthNotification>
{
    public void Process(HealthNotification message)
    {
        Callable.From(
            () => NetworkedEntity.GetByRemoteId(message.EntityId)?.SetServerHealthData(message.PercentHealth)
        )
        .CallDeferred();
    }
}
