using Irrelephant.Outcast.Protocol.Abstractions;
using Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Server.Networking;
using Irrelephant.Outcast.Server.Protocol.Client;
using Microsoft.Extensions.Logging;

namespace Irrelephant.Outcast.Server.Protocol;

public class ServerSideProtocolHandler(IClient client, ILogger logger) : IProtocolHandler
{
    public void HandleNewInboundMessage(object? sender, Message message)
    {
        var messageQueue = (IoSocketMessageHandler)sender!;
        logger.LogDebug("Inbound message received: {Message}", message);

        if (message is ConnectRequest connectRequest && client is ServerSideConnectingClient connectingClient)
        {
            connectingClient.ClientName = connectRequest.Name;
            messageQueue.EnqueueMessage(
                new ConnectResponse(
                    AcceptedName: connectRequest.Name,
                    client.SessionId
                )
            );
        }
    }
}
