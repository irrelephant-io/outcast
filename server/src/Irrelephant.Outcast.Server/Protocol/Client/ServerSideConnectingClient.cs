using Irrelephant.Outcast.Protocol.Abstractions;
using Irrelephant.Outcast.Server.Configuration;
using Irrelephant.Outcast.Server.Networking;
using Microsoft.Extensions.Options;

namespace Irrelephant.Outcast.Server.Protocol.Client;

public class ServerSideConnectingClient : IClient
{
    /// <summary>
    /// Connecting client communicates via TCP, so message IO has to be managed with a message handler.
    /// </summary>
    public IMessageHandler MessageHandler { get; }

    /// <summary>
    /// Controls the overall state of the client, sending and receiving messages to maintain protocol communications.
    /// </summary>
    public IProtocolHandler ProtocolHandler { get; }

    public Guid SessionId { get; set; } = Guid.CreateVersion7();
    public string ClientName { get; set; } = "<unknown>";

    public ServerSideConnectingClient(IOptions<NetworkingOptions> options, IoSocketMessageHandler messageHandler)
    {
        MessageHandler = messageHandler;
        ProtocolHandler = new ServerSideProtocolHandler(this, options.Value.Logger);
        MessageHandler.InboundMessageReceived += ProtocolHandler.HandleNewInboundMessage;
    }

    public void Dispose()
    {
        MessageHandler.InboundMessageReceived -= ProtocolHandler.HandleNewInboundMessage;
    }
}

public interface IClient : IDisposable
{
    Guid SessionId { get; set; }

    string ClientName { get; set; }

    public IMessageHandler MessageHandler { get; }

    public IProtocolHandler ProtocolHandler { get; }
}
