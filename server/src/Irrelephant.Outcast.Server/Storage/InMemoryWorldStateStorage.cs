using System.Collections.Concurrent;
using Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Messages.Primitives;
using Irrelephant.Outcast.Protocol.Networking.Session;
using Irrelephant.Outcast.Server.Storage.Models;

namespace Irrelephant.Outcast.Server.Storage;

public class InMemoryWorldStateStorage : IWorldStateStorage
{
    private readonly Vector3Primitive _defaultSpawnPosition = new(-173f, 0, -112f);

    private readonly IDictionary<string, EntityDescriptor> _storage
        = new ConcurrentDictionary<string, EntityDescriptor>();

    private readonly IDictionary<Guid, PlayerEntityDescriptor> _activePlayers =
        new ConcurrentDictionary<Guid, PlayerEntityDescriptor>();

    private readonly IList<EntityDescriptor> _worldEntities =
    [
        new()
        {
            EntityId = Guid.NewGuid(),
            Position = new Vector3Primitive(-159, 0, -99f),
            YAxisRotation = 22.9f
        },
        new()
        {
            EntityId = Guid.NewGuid(),
            Position = new Vector3Primitive(-156, 0, -97.5f),
            YAxisRotation = 22.9f
        }
    ];

    public PlayerEntityDescriptor LoadAndActivatePlayer(IClient client)
    {
        if (!_storage.ContainsKey(client.ClientName))
        {
            _storage.Add(
                client.ClientName,
                new EntityDescriptor
                {
                    EntityId = Guid.NewGuid(),
                    Position = _defaultSpawnPosition,
                    YAxisRotation = 0
                }
            );
        }

        var baseDescriptor = _storage[client.ClientName];
        var playerDescriptor = new PlayerEntityDescriptor
        {
            Client = client,
            EntityId = baseDescriptor.EntityId,
            Position = baseDescriptor.Position,
            YAxisRotation = baseDescriptor.YAxisRotation
        };

        if (!_activePlayers.TryGetValue(baseDescriptor.EntityId, out var player))
        {
            _activePlayers[baseDescriptor.EntityId] = playerDescriptor;
        }
        else
        {
            player.Client = client;
        }

        return playerDescriptor;
    }

    public void UnloadAndDeactivatePlayer(PlayerEntityDescriptor player)
    {
        _activePlayers.Remove(player.EntityId);
        _storage[player.Client.ClientName] = new EntityDescriptor
        {
            EntityId = player.EntityId,
            Position = player.Position,
            YAxisRotation = player.YAxisRotation
        };
    }

    public IEnumerable<EntityDescriptor> GetWorldEntities() => _worldEntities;

    public IEnumerable<PlayerEntityDescriptor> GetActivePlayers() => _activePlayers.Values;
}
