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
    public void Initialize()
    {
        EnqueueOutboundMessage(new ConnectRequest(playerName));
    }
}
