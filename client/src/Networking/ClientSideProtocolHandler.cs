using System;
using Godot;
using Irrelephant.Outcast.Client.Networking.Extensions;
using Irrelephant.Outcast.Client.Ui.Control;
using Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Protocol.Networking;
using Irrelephant.Outcast.Protocol.Networking.Session;

namespace Irrelephant.Outcast.Client.Networking;

public class ClientSideProtocolHandler(IMessageHandler messageHandler, string name) : IProtocolHandler
{
    private Guid _sessionId = Guid.Empty;

    public void HandleNewInboundMessage(object? sender, Message message)
    {
        if (message is ConnectResponse connectResponse)
        {
            _sessionId = connectResponse.SessionId;
            GD.Print($"Session established: {_sessionId}");

            var entity = NetworkedEntity.Spawn<Player>(
                connectResponse.SpawnPosition.ToVector3(),
                connectResponse.YAxisRotation,
                this
            );
            entity.RemoteId = connectResponse.EntityId;
            PlayerController.Instance.SetPlayerEntity(entity);

            GD.Print("Player spawned!");
        }
        else if (message is SpawnEntity spawnEntity)
        {
            var entity = NetworkedEntity.Spawn<NetworkedEntity>(
                spawnEntity.SpawnPosition.ToVector3(),
                spawnEntity.YAxisRotation,
                this
            );
            entity.RemoteId = spawnEntity.EntityId;

            GD.Print($"Entity spawned: {spawnEntity.EntityId}");
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public void HandleAfterTransportConnected()
    {
        messageHandler.EnqueueMessage(new ConnectRequest(name));
    }

    public void HandleBeforeTransportDisconnected()
    {
        // No-op
    }
}
