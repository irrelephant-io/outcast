using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Irrelephant.Outcast.Server.Data.Contract;
using Irrelephant.Outcast.Server.Data.Storage;

namespace Irrelephant.Outcast.Client.GameData;

public partial class GameDataRegistry : Node
{
    public static GameDataRegistry Instance { get; private set; } = null!;

    private readonly Dictionary<Guid, EntityArchetype> _archetypes = new();

    private readonly StorageReader _storageReader = new();

    public EntityArchetype? GetArchetypeById(Guid id) =>
        _archetypes.ContainsKey(id) ? _archetypes[id] : null;

    public override void _EnterTree()
    {
        Instance = this;
        foreach (var archetype in _storageReader.ReadArchetypesAsync().ToBlockingEnumerable())
        {
            _archetypes.Add(archetype.Id, archetype);
        }

        base._EnterTree();
    }
}
