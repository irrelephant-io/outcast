using Godot;
using Irrelephant.Outcast.Client.Networking.Extensions;
using Irrelephant.Outcast.Client.Ui.Control;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

namespace Irrelephant.Outcast.Client.Networking;

public partial class NetworkServiceUpdate : Node
{
    private NetworkService _networkService = null!;

    [Export] public PlayerController PlayerController { get; set; } = null!;

    public override void _Ready()
    {
        _networkService = GetNode<NetworkService>("..");
    }

    public override void _Process(double delta)
    {
        if (_networkService.Client is null)
        {
            return;
        }

        while (_networkService.Client.TryDequeueInboundMessage(out var message))
        {
            if (message is ConnectResponse response)
            {
                var spawned = NetworkedEntity.Spawn<Player>(
                    response.EntityId,
                    response.SpawnPosition.ToGodotVector(),
                    response.YAxisRotation,
                    _networkService.Client
                );
                PlayerController.SetPlayerEntity(spawned);
                GD.Print("Spawned player: " + spawned.RemoteId);
            }
            else if (message is EntityPositionUpdate position)
            {
                var node = GetNode(new NodePath($"/root/Main/World/Entities/{position.EntityId:N}"));
                if (node is Node3D node3d)
                {
                    node3d.SetPosition(position.MovePosition.ToGodotVector());
                }
            }
            else if (message is SpawnEntity genericEntitySpawn)
            {
                NetworkedEntity.Spawn<NetworkedEntity>(
                    genericEntitySpawn.EntityId,
                    genericEntitySpawn.SpawnPosition.ToGodotVector(),
                    genericEntitySpawn.YAxisRotation,
                    _networkService.Client
                );
            }
        }

        base._Process(delta);
    }
}
