using Godot;
using Irrelephant.Outcast.Client.Entities;
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
                var spawned = NetworkedEntity.Spawn<PlayerEntity>(
                    response.EntityId,
                    response.SpawnPosition.ToGodotVector(),
                    response.YAxisRotation,
                    _networkService.Client
                );
                _networkService.Client.SessionId = response.SessionId;
                PlayerController.SetPlayerEntity(spawned);
                spawned.SetServerPositionalData(
                    response.SpawnPosition.ToGodotVector(),
                    response.YAxisRotation
                );
                spawned.SetEntityName(_networkService.Client.PlayerName);
                GD.Print("Spawned player: " + spawned.RemoteId);
            }
            else if (message is EntityPositionUpdate position)
            {
                var node = GetNode($"/root/Main/World/Entities/{position.EntityId:N}");
                if (node is NetworkedEntity networkedEntity)
                {
                    networkedEntity.SetServerPositionalData(
                        position.CurrentPosition.ToGodotVector(),
                        position.CurrentYAxisRotation
                    );
                }
            }
            else if (message is SpawnPlayerEntity playerSpawn)
            {
                var player = NetworkedEntity.Spawn<PlayerEntity>(
                    playerSpawn.EntityId,
                    playerSpawn.SpawnPosition.ToGodotVector(),
                    playerSpawn.YAxisRotation,
                    _networkService.Client
                );
                player.SetServerPositionalData(
                    playerSpawn.SpawnPosition.ToGodotVector(),
                    playerSpawn.YAxisRotation
                );
                player.SetEntityName(playerSpawn.PlayerName);
            }
            else if (message is InitiateMoveNotice initiateMoveNotice)
            {
                var node = GetNode($"/root/Main/World/Entities/{initiateMoveNotice.EntityId:N}");
                if (node is Entity entity)
                {
                    entity.NotifyMovementStart();
                }
            }
            else if (message is MoveDoneNotice moveDoneNotice)
            {
                var node = GetNode($"/root/Main/World/Entities/{moveDoneNotice.EntityId:N}");
                if (node is Entity entity)
                {
                    entity.NotifyMovementEnd();
                }
            }
            else if (message is SpawnEntity genericEntitySpawn)
            {
                var entity = NetworkedEntity.Spawn<NpcEntity>(
                    genericEntitySpawn.EntityId,
                    genericEntitySpawn.SpawnPosition.ToGodotVector(),
                    genericEntitySpawn.YAxisRotation,
                    _networkService.Client
                );
                entity.SetServerPositionalData(
                    genericEntitySpawn.SpawnPosition.ToGodotVector(),
                    genericEntitySpawn.YAxisRotation
                );
            }
            else if (message is DespawnEntity despawnEntity)
            {
                var node = GetNode(new NodePath($"/root/Main/World/Entities/{despawnEntity.EntityId:N}"));
                node.QueueFree();
            }
        }

        base._Process(delta);
    }
}
