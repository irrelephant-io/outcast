using System.Numerics;

namespace Irrelephant.Outcast.Server.Simulation.Components;

public struct NamedEntity
{
    public string Name;
}

public struct GlobalId : IComponent
{
    public Guid Id;
}

public struct Movable : IComponent
{
    public bool IsMoved;
    public Vector3 Position;
    public float MoveSpeed;
}

public struct Transform : IComponent
{
    public Vector3 Position;
    public Vector3 Rotation;
}
