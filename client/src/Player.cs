using Godot;
using Irrelephant.Outcast.Client.Networking;

namespace Irrelephant.Outcast.Client;

public partial class Player : NetworkedEntity
{
    [Export]
    public Vector3 DesiredPosition { get; set; }

    private Vector3 _startPos;

    public override void _Ready()
    {
        _startPos = Position;
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
//        Transform = Transform.Translated(Vector3.Left * (float)(Math.Sin(Time.GetUnixTimeFromSystem())));
        base._PhysicsProcess(delta);
    }
}
