using System;
using Godot;
using Irrelephant.Outcast.Client.Simulation;

namespace Irrelephant.Outcast.Client.Entities;

public partial class NetworkedEntityBase : Node3D
{
    public Guid RemoteId { get; set; }

    public required Networking.Client OwningClient { get; set; }

    [Export]
    public Label3D EntityLabel { get; set; } = null!;

    public static TEntity Spawn<TEntity>(
        Guid remoteId,
        Vector3 position,
        float yRotation,
        Networking.Client owningClient
    )
        where TEntity : NetworkedEntityBase
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

    public void SetEntityName(string entityName)
    {
        EntityLabel.Text = entityName;
    }
}
