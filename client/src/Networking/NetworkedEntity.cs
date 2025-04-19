using System;
using Godot;
using Irrelephant.Outcast.Client.Simulation;
using Irrelephant.Outcast.Protocol.Networking.Session;

namespace Irrelephant.Outcast.Client.Networking;

public partial class NetworkedEntity : Node3D
{
    public Guid RemoteId { get; set; }

    public required IProtocolHandler ProtocolHandler { get; set; }

    public static TEntity Spawn<TEntity>(
        Vector3 position,
        float yRotation,
        IProtocolHandler owningHandler
    )
        where TEntity : NetworkedEntity
    {
        var template = GD.Load<PackedScene>($"res://components/entities/{typeof(TEntity).Name}.tscn");
        var instance = (TEntity)template.Instantiate(PackedScene.GenEditState.Instance);
        instance.SetPosition(position);
        instance.SetRotation(Vector3.Up * yRotation);
        instance.ProtocolHandler = owningHandler;

        Callable.From(() => NetworkEntityContainer.Node.AddChild(instance)).CallDeferred();
        return instance;
    }
}
