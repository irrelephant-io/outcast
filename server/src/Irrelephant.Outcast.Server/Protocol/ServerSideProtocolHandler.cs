using Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Protocol.Networking;
using Irrelephant.Outcast.Protocol.Networking.Session;
using Irrelephant.Outcast.Server.Protocol.Client;
using Irrelephant.Outcast.Server.Storage;
using Irrelephant.Outcast.Server.Storage.Models;
using Microsoft.Extensions.Logging;

namespace Irrelephant.Outcast.Server.Protocol;

public class ServerSideProtocolHandler(
    IClient client,
    IWorldStateStorage worldStateStorage,
    ILogger logger
) : IProtocolHandler
{
    private PlayerEntityDescriptor? _playerEntityDescriptor;

    public void HandleNewInboundMessage(object? sender, Message message)
    {
        var messageQueue = (IMessageHandler)sender!;
        logger.LogDebug("Inbound message received: {Message}", message);

        if (message is ConnectRequest connectRequest && client is ServerSideConnectingClient connectingClient)
        {
            connectingClient.ClientName = connectRequest.Name;

            var activePlayers = worldStateStorage.GetActivePlayers().ToArray();
            _playerEntityDescriptor = worldStateStorage.LoadAndActivatePlayer(client);

            foreach (var activePlayer in activePlayers)
            {
                activePlayer.Client.MessageHandler.EnqueueMessage(
                    new SpawnEntity(
                        EntityId: _playerEntityDescriptor.EntityId,
                        SpawnPosition: _playerEntityDescriptor.Position,
                        YAxisRotation: _playerEntityDescriptor.YAxisRotation
                    )
                );
            }

            messageQueue.EnqueueMessage(
                new ConnectResponse(
                    AcceptedName: connectRequest.Name,
                    client.SessionId,
                    _playerEntityDescriptor.EntityId,
                    _playerEntityDescriptor.Position,
                    _playerEntityDescriptor.YAxisRotation
                )
            );

            foreach (var activePlayer in activePlayers)
            {
                messageQueue.EnqueueMessage(
                    new SpawnEntity(
                        EntityId: activePlayer.EntityId,
                        SpawnPosition: activePlayer.Position,
                        YAxisRotation: activePlayer.YAxisRotation
                    )
                );
            }

            foreach (var worldEntity in worldStateStorage.GetWorldEntities())
            {
                messageQueue.EnqueueMessage(
                    new SpawnEntity(
                        worldEntity.EntityId,
                        worldEntity.Position,
                        worldEntity.YAxisRotation
                    )
                );
            }
        }
    }

    public void HandleAfterTransportConnected()
    {
        // No-op
    }

    public void HandleBeforeTransportDisconnected()
    {
        if (_playerEntityDescriptor is not null)
        {
            logger.LogDebug(
                "Saving player for session {SessionId}",
                _playerEntityDescriptor.Client.SessionId
            );
            worldStateStorage.UnloadAndDeactivatePlayer(_playerEntityDescriptor);
        }
    }
}
