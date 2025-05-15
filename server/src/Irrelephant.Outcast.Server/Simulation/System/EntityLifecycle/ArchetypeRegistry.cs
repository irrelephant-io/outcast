using System.Numerics;
using Arch.Core;
using Irrelephant.Outcast.Server.Data.Storage;
using Irrelephant.Outcast.Server.Simulation.Components.Ai;
using Irrelephant.Outcast.Server.Simulation.Components.Behavioral;
using Irrelephant.Outcast.Server.Simulation.Components.Data;
using Microsoft.Extensions.Logging;
using EntityArchetype = Irrelephant.Outcast.Server.Data.Contract.EntityArchetype;

namespace Irrelephant.Outcast.Server.Simulation.System.EntityLifecycle;

public class ArchetypeRegistry(
    ILogger logger,
    StorageReader storageReader
)
{
    public static Guid PlayerArchetypeId = new("00000000-0000-0000-0001-0f11d2247838");
    private readonly Dictionary<Guid, EntityArchetype> _archetypes = new();

    public async Task LoadAsync()
    {
        _archetypes.Clear();
        await foreach (var archetype in storageReader.ReadArchetypesAsync())
        {
            _archetypes.Add(archetype.Id, archetype);
        }
        logger.LogInformation("Loaded `{ArchetypeCount}` entity archetypes.", _archetypes.Count);
    }

    public void SetArchetype(
        ref Entity entity,
        Guid archetypeId,
        Vector3 anchorPosition
    )
    {
        var archetype = _archetypes[archetypeId];
        entity.Add(
            new EntityName { Name = archetype.Name },
            new Health { MaxHealth = archetype.MaxHealth, CurrentHealth = archetype.MaxHealth },
            new Movement { MoveSpeed = 2f, TargetPosition = anchorPosition },
            new Attack
            {
                Damage = archetype.AttackDamage,
                AttackCooldownRemaining = 0,
                AttackCooldown = archetype.AttackCooldown,
                Range = 2.0f
            }
        );

        var behaviorBase = new Behavior
        {
            AnchorPosition = anchorPosition,
            ThinkingCooldown = 100,
            ThinkingCooldownVariance = 10,
            RoamDistance = 15,
            PursuitDistance = 30
        };

        if (archetype.IsAggressive)
        {
            entity.Add(
                behaviorBase,
                new AggressiveBehavior()
            );
        }
        else
        {
            entity.Add(
                behaviorBase,
                new PassiveBehavior()
            );
        }
    }
}
