using Arch.Core;
using Arch.Core.Utils;
using Irrelephant.Outcast.Server.Simulation.Components;
using Irrelephant.Outcast.Server.Simulation.Space;
using Irrelephant.Outcast.Server.Simulation.System;
using Irrelephant.Outcast.Server.Simulation.System.Networking;
using Irrelephant.Outcast.Server.Storage;
using Microsoft.Extensions.Logging;
using Schedulers;

namespace Irrelephant.Outcast.Server.Simulation;

public class OutcastWorld : IDisposable
{
    private readonly World _world;

    private readonly IPositionTracker _positionTracker;

    private readonly UpdateInterestSphereSystem _updateInterestSphereSystem;
    private readonly ProcessNetworkMessagesSystem _processNetworkMessagesSystem;

    private readonly JobScheduler _scheduler = new(
        new JobScheduler.Config
        {
            ThreadPrefixName = "Outcast.Server",
            MaxExpectedConcurrentJobs = 64
        }
    );

    private readonly ILogger<OutcastWorld> _logger;
    private readonly StorageReader _storageReader;

    public OutcastWorld(ILogger<OutcastWorld> logger, StorageReader storageReader)
    {
        _logger = logger;
        _storageReader = storageReader;
        logger.LogInformation("Initialising Outcast world...");
        _world = World.Create();
        _positionTracker = new NaivePositionTracker();

        _updateInterestSphereSystem = new UpdateInterestSphereSystem(_positionTracker);
        _processNetworkMessagesSystem = new ProcessNetworkMessagesSystem(_positionTracker);
    }

    public async Task LoadWorldAsync()
    {
        _logger.LogInformation("Loading preset entities.");
        int loadedEntities = 0;

        await foreach (var (gid, transform) in _storageReader.ReadEntitiesAsync("entities.0.json"))
        {
            loadedEntities++;
            var entity = _world.Create(gid, transform);
            _positionTracker.Track(entity);
        }

        _logger.LogInformation("Done loading {EntityCount} entities.", loadedEntities);
    }

    public void AttachClient(Networking.ProtocolClient client)
    {
        _world.Create(
            new ProtocolClient(client)
        );
    }

    public void Simulate()
    {
        _updateInterestSphereSystem.RunEarlyUpdate(_world);
        _processNetworkMessagesSystem.Run(_world);
        ManageEntitiesInInterestSphere.Run(_world);
        MoveCharacters.Run(_world, 0.1f);
        UpdateNetworkCharacterStatus.Run(_world);
        _updateInterestSphereSystem.RunLateUpdate(_world);
    }

    public void Dispose()
    {
        _world.Dispose();
        _scheduler.Dispose();
    }
}
