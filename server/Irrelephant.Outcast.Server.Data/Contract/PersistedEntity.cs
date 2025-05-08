using System.Numerics;
using System.Runtime.Serialization;

namespace Irrelephant.Outcast.Server.Data.Contract;

[DataContract]
public class PersistedEntityTransform
{
    [DataMember(Name = "position", IsRequired = true)]
    public required Vector3 Position { get; set; }

    [DataMember(Name = "rotation")]
    public Vector3 Rotation { get; set; }
}

[DataContract]
public class PersistedEntity
{
    [DataMember(Name = "archetypeId", IsRequired = true)]
    public Guid ArchetypeId { get; set; }

    [DataMember(Name = "transform", IsRequired = true)]
    public required PersistedEntityTransform Transform { get; set; }
}
