using Godot;

namespace Irrelephant.Outcast.Client.Simulation;

public partial class NetworkEntityContainer : Node
{
    public static Node Node { get; private set; }

    public override void _Ready()
    {
        Node = this;
        base._Ready();
    }
}
