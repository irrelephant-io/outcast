using Irrelephant.Outcast.Client.Entities;
using Irrelephant.Outcast.Client.Networking.Extensions;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

namespace Irrelephant.Outcast.Client.Networking.MessageProcessing;

public class SpawnPlayerEntityHandler : IMessageHandler<SpawnPlayerEntity>
{
    public void Process(SpawnPlayerEntity message)
    {
        var player = NetworkedEntity.Spawn<PlayerEntity>(
            message.EntityId,
            message.SpawnPosition.ToGodotVector(),
            message.YAxisRotation
        );
        player.SetServerPositionalData(
            message.SpawnPosition.ToGodotVector(),
            message.YAxisRotation
        );
        player.SetEntityName(message.PlayerName);
    }
}

public class SpawnEntityHandler : IMessageHandler<SpawnEntity>
{
    public void Process(SpawnEntity message)
    {
        var entity = NetworkedEntity.Spawn<NpcEntity>(
            message.EntityId,
            message.SpawnPosition.ToGodotVector(),
            message.YAxisRotation
        );
        entity.SetServerPositionalData(
            message.SpawnPosition.ToGodotVector(),
            message.YAxisRotation
        );
        entity.SetEntityName("Some Random Mob");
    }
}

public class DespawnEntityHandler : IMessageHandler<DespawnEntity>
{
    public void Process(DespawnEntity message)
    {
        NetworkedEntity.GetByRemoteId(message.EntityId)?.QueueFree();
    }
}
