using System;
using Irrelephant.Outcast.Protocol.Networking;
using Irrelephant.Outcast.Protocol.Networking.Session;

namespace Irrelephant.Outcast.Client.Networking;

public class Client : IClient
{
    public Guid SessionId { get; set; }
    public required string ClientName { get; set; }

    private readonly IMessageHandler? _messageHandler;
    public required IMessageHandler MessageHandler {
        get => _messageHandler ?? throw new InvalidOperationException("Client is not yet wired up!");
        init
        {
            if (_messageHandler is not null && _protocolHandler != null)
            {
                _messageHandler.InboundMessageReceived -= _protocolHandler.HandleNewInboundMessage;
            }
            _messageHandler = value;
            if (_protocolHandler is not null)
            {
                _messageHandler.InboundMessageReceived += _protocolHandler.HandleNewInboundMessage;
            }
        }
    }

    private readonly IProtocolHandler? _protocolHandler;
    public required IProtocolHandler ProtocolHandler {
        get => _protocolHandler ?? throw new InvalidOperationException("Client is not yet wired up!");
        init
        {
            if (_protocolHandler is not null && _messageHandler is not null)
            {
                _messageHandler.InboundMessageReceived -= _protocolHandler.HandleNewInboundMessage;
            }
            _protocolHandler = value;
            if (_messageHandler is not null)
            {
                _messageHandler.InboundMessageReceived += _protocolHandler.HandleNewInboundMessage;
            }

        }
    }

    public void Dispose()
    {
        if (_messageHandler is not null && _protocolHandler is not null)
        {
            _messageHandler.InboundMessageReceived -= _protocolHandler.HandleNewInboundMessage;
        }
    }
}
