using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Irrelephant.Outcast.Server.Simulation.Components;
using Irrelephant.Outcast.Server.Simulation.Components.Behavioral;

namespace Irrelephant.Outcast.Server.Simulation.Space;

public interface IPositionTracker
{
    HashSet<Entity> QueryWithin(Vector3 position, float distance);

    void Track(Entity entity);

    void Untrack(Entity entity);
}

public class NaivePositionTracker : IPositionTracker
{
    public IList<Entity> _allKnownEntities = new List<Entity>();

    public HashSet<Entity> QueryWithin(Vector3 position, float distance) =>
        _allKnownEntities
            .Where(it => {
                ref var transform = ref it.Get<Transform>();
                return (transform.Position - position).LengthSquared() < distance * distance;
            })
            .ToHashSet();

    public void Track(Entity entity)
    {
        _allKnownEntities.Add(entity);
    }

    public void Untrack(Entity entity)
    {
        _allKnownEntities.Remove(entity);
    }
}
