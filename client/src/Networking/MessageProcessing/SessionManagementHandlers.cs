using Irrelephant.Outcast.Client.Entities;
using Irrelephant.Outcast.Client.Networking.Extensions;
using Irrelephant.Outcast.Client.Ui.Control;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

namespace Irrelephant.Outcast.Client.Networking.MessageProcessing;

public class ConnectResponseHandler : IMessageHandler<ConnectResponse>
{
    public void Process(ConnectResponse message)
    {
        var client = NetworkService.Instance.Client!;
        var spawned = NetworkedEntity.Spawn<PlayerEntity>(
            message.EntityId,
            message.SpawnPosition.ToGodotVector(),
            message.YAxisRotation
        );
        client.SessionId = message.SessionId;
        PlayerController.Instance.SetPlayerEntity(spawned);
        spawned.SetServerPositionalData(
            message.SpawnPosition.ToGodotVector(),
            message.YAxisRotation
        );
        spawned.SetEntityName(client.PlayerName);
    }
}

public class DisconnectNotificationHandler : IMessageHandler<DisconnectNotification>
{
    public void Process(DisconnectNotification message)
    {
        throw new System.NotImplementedException();
    }
}
