using Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Messages.Primitives;
using Irrelephant.Outcast.Protocol.Networking.Session;

namespace Irrelephant.Outcast.Server.Storage.Models;

public class EntityDescriptor
{
    public Guid EntityId { get; set; }

    public Vector3Primitive Position { get; set; }

    public float YAxisRotation { get; set; }
}

public class PlayerEntityDescriptor : EntityDescriptor
{
    public IClient Client { get; set; }
}
