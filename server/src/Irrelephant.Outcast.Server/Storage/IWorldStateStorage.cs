using Irrelephant.Outcast.Protocol.Networking.Session;
using Irrelephant.Outcast.Server.Storage.Models;

namespace Irrelephant.Outcast.Server.Storage;

public interface IWorldStateStorage
{
    PlayerEntityDescriptor LoadAndActivatePlayer(IClient client);

    void UnloadAndDeactivatePlayer(PlayerEntityDescriptor player);

    IEnumerable<EntityDescriptor> GetWorldEntities();

    IEnumerable<PlayerEntityDescriptor> GetActivePlayers();
}
