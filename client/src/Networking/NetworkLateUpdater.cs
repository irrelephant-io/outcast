using System;
using Godot;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

namespace Irrelephant.Outcast.Client.Networking;

public partial class NetworkLateUpdater : Node
{
    private NetworkService _networkService = null!;

    public override void _Ready()
    {
        _networkService = GetNode<NetworkService>("..");
    }

    public override void _Process(double delta)
    {
        _networkService.Client?.Transmit();
    }

    public override void _ExitTree()
    {
        if (_networkService.Client?.SessionId is { } sessionId && sessionId != Guid.Empty)
        {
            _networkService.Client?.EnqueueOutboundMessage(new DisconnectNotification(sessionId, "Exiting."));
        }
    }
}
