using Arch.Core;
using Irrelephant.Outcast.Server.Simulation.Components.Behavioral;
using Irrelephant.Outcast.Server.Simulation.Components.Communication;
using Irrelephant.Outcast.Server.Simulation.Components.Data;
using Irrelephant.Outcast.Server.Simulation.Space;

namespace Irrelephant.Outcast.Server.Simulation.System.Networking;

public class UpdateInterestSphereSystem(IPositionTracker positionTracker)
{
    public void RunEarlyUpdate(World world)
    {
        var allNetworkedEntities = new QueryDescription().WithAll<ProtocolClient, Transform>();
        world.Query(
            in allNetworkedEntities,
            (Entity entity, ref ProtocolClient protocolClient, ref Transform transform) =>
            {
                var entitiesInSphere = positionTracker
                    .QueryWithin(transform.Position, protocolClient.InterestSphere.Radius);
                var sphere = protocolClient.InterestSphere;

                foreach (var entityInSphere in entitiesInSphere)
                {
                    if (entityInSphere == entity)
                    {
                        continue;
                    }

                    if (!sphere.EnteringEntities.Contains(entityInSphere)
                        && !sphere.EntitiesWithin.Contains(entityInSphere))
                    {
                        sphere.EnteringEntities.Add(entityInSphere);
                    }
                }

                foreach (var entityWithin in sphere.EntitiesWithin)
                {
                    if (entityWithin.Has<GlobalId, DespawnMarker>() || !entitiesInSphere.Contains(entityWithin))
                    {
                        sphere.LeavingEntities.Add(entityWithin);
                    }
                }
                sphere.EntitiesWithin.ExceptWith(sphere.LeavingEntities);
            }
        );
    }

    public void RunLateUpdate(World world)
    {
        var allNetworkedEntities = new QueryDescription().WithAll<ProtocolClient, Transform>();
        world.Query(
            in allNetworkedEntities,
            (ref ProtocolClient protocolClient) =>
            {
                protocolClient.InterestSphere.EntitiesWithin.UnionWith(protocolClient.InterestSphere.EnteringEntities);
                protocolClient.InterestSphere.EnteringEntities.Clear();
                protocolClient.InterestSphere.LeavingEntities.Clear();
            }
        );
    }
}
