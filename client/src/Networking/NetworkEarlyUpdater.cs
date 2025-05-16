using Godot;

namespace Irrelephant.Outcast.Client.Networking;

public partial class NetworkEarlyUpdater : Node
{
    private NetworkService _networkService = null!;

    public override void _Ready()
    {
        _networkService = GetNode<NetworkService>("..");
    }

    public override void _Process(double delta)
    {
        _networkService.Client?.Receive();
    }
}
