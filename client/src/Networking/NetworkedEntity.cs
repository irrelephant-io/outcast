using System;
using Godot;
using Irrelephant.Outcast.Client.Simulation;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

namespace Irrelephant.Outcast.Client.Networking;

public partial class NetworkedEntity : Node3D
{
    public Guid RemoteId { get; set; }

    public required Client OwningClient { get; set; }

    public static TEntity Spawn<TEntity>(
        Guid remoteId,
        Vector3 position,
        float yRotation,
        Client owningClient
    )
        where TEntity : NetworkedEntity
    {
        var template = GD.Load<PackedScene>($"res://components/entities/{typeof(TEntity).Name}.tscn");
        var instance = (TEntity)template.Instantiate(PackedScene.GenEditState.Instance);
        instance.SetName(remoteId.ToString("N"));
        instance.SetPosition(position);
        instance.SetRotation(Vector3.Up * yRotation);
        instance.RemoteId = remoteId;
        instance.OwningClient = owningClient;

        Callable.From(() => NetworkEntityContainer.Node.AddChild(instance)).CallDeferred();
        return instance;
    }
}
