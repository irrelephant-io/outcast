using System.Collections;
using Arch.Core;

namespace Irrelephant.Outcast.Server.Simulation.Components.Ai;

public class ThreatTable : IEnumerable<Entity>
{
    private readonly Dictionary<Entity, int> _threatList = new();
    public IEnumerator<Entity> GetEnumerator() => _threatList
        .OrderByDescending(it => it.Value)
        .Select(it => it.Key)
        .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool IsThreatened => _threatList.Count > 0;

    public void AddThreat(Entity entity, int threat)
    {
        if (_threatList.TryGetValue(entity, out _))
        {
            _threatList[entity] += threat;
        }
        else if (threat > 0)
        {
            _threatList.Add(entity, threat);
        }
    }

    public void DeleteThreat(Entity entity) => _threatList[entity] = 0;

    public void DecayThreat(Entity entity)
    {
        if (_threatList.TryGetValue(entity, out var currentThreat))
        {
            var newThreat = currentThreat - Math.Max(1, (int)(currentThreat * 0.1));
            _threatList[entity] = newThreat;
        }
    }

    public void CleanupTable()
    {
        var emptyKeys = _threatList.Where(it => it.Value <= 0).ToArray();
        foreach (var keyToRemove in emptyKeys)
        {
            _threatList.Remove(keyToRemove.Key);
        }
    }
}
