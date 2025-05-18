using System.Runtime.Serialization;

namespace Irrelephant.Outcast.Server.Data.Contract;

[DataContract]
public class EntityArchetype
{
    [DataMember(Name = "id", IsRequired = true)]
    public Guid Id { get; set; }

    [DataMember(Name = "maxHealth", IsRequired = true)]
    public int MaxHealth { get; set; }

    [DataMember(Name = "isAggressive", IsRequired = true)]
    public bool IsAggressive { get; set; }

    [DataMember(Name = "movementSpeed", IsRequired = true)]
    public float MovementSpeed { get; set; }

    [DataMember(Name = "attackDamage", IsRequired = true)]
    public int AttackDamage { get; set; }

    [DataMember(Name = "attackCooldown", IsRequired = true)]
    public int AttackCooldown { get; set; }

    [DataMember(Name = "level", IsRequired = true)]
    public int Level { get; set; }

    [DataMember(Name = "name", IsRequired = true)]
    public required string Name { get; set; }

    [DataMember(Name = "respawnTime", IsRequired = true)]
    public required int RespawnTime { get; set; }
}
