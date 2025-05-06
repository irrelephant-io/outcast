using System.Numerics;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Server.Simulation.Components;
using Irrelephant.Outcast.Server.Simulation.Space;

namespace Irrelephant.Outcast.Server.Simulation.System.EntityLifecycle;

public class EntitySpawner(World world, IPositionTracker positionTracker)
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
            new GlobalId { Id = entityId },
            new Transform { Position = DefaultSpawnPosition },
            new Movable { Position = DefaultSpawnPosition, MoveSpeed = 5.0f },
            new NamedEntity { Name = name }
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
                        new DisconnectNotice(SessionId: gid.Id, "Disconnected from server")
                    );
                }
            }
        }

        commandBuffer.Add(entity, new DespawnMarker());
        positionTracker.Untrack(entity);
    }
}
