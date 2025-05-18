using System;
using Godot;
using Irrelephant.Outcast.Client.GameData;
using Irrelephant.Outcast.Client.Simulation;

namespace Irrelephant.Outcast.Client.Entities;

public partial class NetworkedEntity : Entity
{
    public Guid RemoteId { get; set; }

    public Vector3 LastServerPosition { get; set; }
    public float LastServerYRotation { get; set; }
    public string EntityName { get; set; } = "";

    private Label3D? _entityNameLabel;

    public override void _EnterTree()
    {
        _entityNameLabel = (Label3D)GetNode("EntityLabel");
    }

    public static TEntity Spawn<TEntity>(
        Guid remoteId,
        Vector3 position,
        float yRotation,
        Guid? archetypeId = null
    )
        where TEntity : NetworkedEntity
    {
        var template = ResourceLoader.Load<PackedScene>($"res://components/entities/{typeof(TEntity).Name}.tscn");
        var instance = (TEntity)template.Instantiate(PackedScene.GenEditState.Instance);
        instance.SetName(remoteId.ToString("N"));
        instance.SetPosition(position);
        instance.SetRotation(Vector3.Up * yRotation);
        instance.RemoteId = remoteId;
        Callable.From(() => NetworkEntityContainer.Node.AddChild(instance)).CallDeferred();

        if (archetypeId.HasValue)
        {
            var archetype = GameDataRegistry.Instance.GetArchetypeById(archetypeId.Value);
            instance.MaxHealth = archetype!.MaxHealth;
            instance.SetEntityName(archetype.Name);
        }

        return instance;
    }

    public void SetEntityName(string entityName)
    {
        EntityName = entityName;
        Callable.From(() => _entityNameLabel?.SetText(entityName)).CallDeferred();
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

    public void SetServerHealthData(int? health)
    {
        if (health != CurrentHealth)
        {
            CurrentHealth = health;
        }
    }

    public static NetworkedEntity GetByRemoteId(Guid remoteId)
    {
        return NetworkEntityContainer.Node.GetNode<NetworkedEntity>(remoteId.ToString("N"));
    }
}
