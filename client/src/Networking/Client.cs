using System;
using Irrelephant.Outcast.Networking.Protocol;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Encoding;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Networking.Transport.Abstractions;

namespace Irrelephant.Outcast.Client.Networking;

public class Client(
    ITransportHandler transportHandler,
    IMessageCodec codec,
    string playerName
) : DefaultProtocolMessageQueue(transportHandler, codec)
{
    public Guid SessionId { get; set; }

    public string PlayerName { get; set; } = null!;

    public void Initialize()
    {
        PlayerName = playerName;
        EnqueueOutboundMessage(new ConnectRequest(playerName));
    }
}
