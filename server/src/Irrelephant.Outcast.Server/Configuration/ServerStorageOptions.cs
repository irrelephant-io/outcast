using Irrelephant.Outcast.Server.Storage;

namespace Irrelephant.Outcast.Server.Configuration;

public class ServerStorageOptions
{
    public required IWorldStateStorage Storage { get; init; }
}
