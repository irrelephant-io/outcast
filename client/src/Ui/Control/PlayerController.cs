using Godot;
using Irrelephant.Outcast.Client.Ui.Camera;

namespace Irrelephant.Outcast.Client.Ui.Control;

public partial class PlayerController : Node
{
    [Export]
    public Node3D ControlledPlayer;

    [Export]
    public CameraController CameraController;

    [Export]
    public float MoveSpeed { get; set; }

    private Vector3 _desiredPosition;

    public override void _Ready()
    {
        _desiredPosition = ControlledPlayer.Position;
        CameraController.OnLeftClick += clickedLocation =>
        {
            _desiredPosition = clickedLocation;
        };

        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        ControlledPlayer.Position = ControlledPlayer.Position.MoveToward(
            _desiredPosition,
            (float)(MoveSpeed * delta)
        );

        base._PhysicsProcess(delta);
    }
}
