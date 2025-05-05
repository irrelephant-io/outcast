using System;
using Godot;
using Irrelephant.Outcast.Client.Simulation;

namespace Irrelephant.Outcast.Client.Entities;

public partial class NetworkedEntity : Entity
{
    public Guid RemoteId { get; set; }

    public required Networking.Client OwningClient { get; set; }

    public Vector3 LastServerPosition { get; set; }
    public float LastServerYRotation { get; set; }

    [Export]
    public Label3D EntityLabel { get; set; } = null!;

    public static TEntity Spawn<TEntity>(
        Guid remoteId,
        Vector3 position,
        float yRotation,
        Networking.Client owningClient
    )
        where TEntity : NetworkedEntity
    {
        var template = ResourceLoader.Load<PackedScene>($"res://components/entities/{typeof(TEntity).Name}.tscn");
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

    public override void _Process(double delta)
    {
        SetPosition(Transform.Origin.Lerp(LastServerPosition, 0.1f));
        var currentRotation = Transform.Basis.GetRotationQuaternion();
        var targetRotation = new Quaternion(Vector3.Up, LastServerYRotation);
        SetBasis(new Basis(currentRotation.Slerp(targetRotation, 0.05f)));
        base._Process(delta);
    }

    public void SetServerPositionalData(
        Vector3 position,
        float yRotation
    )
    {
        LastServerPosition = position;
        LastServerYRotation = yRotation;
    }
}
