using System.Numerics;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Server.Simulation.Components;
using Irrelephant.Outcast.Server.Simulation.Components.Behavioral;
using Irrelephant.Outcast.Server.Simulation.Components.Communication;
using Irrelephant.Outcast.Server.Simulation.Components.Data;
using Irrelephant.Outcast.Server.Simulation.Space;

namespace Irrelephant.Outcast.Server.Simulation.System.EntityLifecycle;

public class EntitySpawner(
    World world,
    ArchetypeRegistry archetypeRegistry,
    IPositionTracker positionTracker
)
{
    private static readonly Vector3 DefaultSpawnPosition = new(467, 1168, -622);

    public void SpawnConnectingPlayer(
        ref Entity entity,
        string name
    )
    {
        var entityId = Guid.NewGuid();
        world.Add(
            entity,
            new GlobalId { Id = entityId, ArchetypeId = ArchetypeRegistry.PlayerArchetypeId },
            new Transform { Position = DefaultSpawnPosition },
            new Movement { TargetPosition = DefaultSpawnPosition, MoveSpeed = 5.0f },
            new EntityName { Name = name },
            new Health { MaxHealth = 100, CurrentHealth = 100 },
            new Attack { Damage = 5, AttackCooldownRemaining = 0, AttackCooldown = 10, Range = 2.0f },
            new State()
        );
        positionTracker.Track(entity);
    }

    public void DespawnPlayer(
        ref Entity entity,
        ref ProtocolClient client,
        CommandBuffer commandBuffer
    )
    {
        ref var gid = ref entity.TryGetRef<GlobalId>(out var hasGid);
        // If no GlobalId is present, then the player didn't finish connecting.
        if (hasGid)
        {
            foreach (var entityInSphereOfInterest in client.InterestSphere.EntitiesWithin)
            {
                ref var interestedClient = ref entityInSphereOfInterest.TryGetRef<ProtocolClient>(out var exists);
                if (exists)
                {
                    interestedClient.Network.EnqueueOutboundMessage(
                        new DisconnectNotification(SessionId: gid.Id, "Disconnected from server")
                    );
                }
            }
        }

        commandBuffer.Add(entity, new DespawnMarker());
        positionTracker.Untrack(entity);
    }

    public void SpawnEntity(
        Guid entityArchetypeId,
        Vector3 position
    )
    {
        var entity = world.Create();
        archetypeRegistry.SetArchetype(ref entity, entityArchetypeId, position);
        entity.Add(
            new GlobalId { Id = Guid.NewGuid(), ArchetypeId = entityArchetypeId },
            new Transform { Position = position },
            new State()
        );
        positionTracker.Track(entity);
    }
}
