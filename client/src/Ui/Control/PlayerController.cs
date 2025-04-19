using Godot;
using Irrelephant.Outcast.Client.Ui.Camera;

namespace Irrelephant.Outcast.Client.Ui.Control;

public partial class PlayerController : Node
{
    public static PlayerController Instance { get; private set; }

    public Player? ControlledPlayer { get; set; }

    [Export]
    public CameraController CameraController = null!;

    [Export]
    public float MoveSpeed { get; set; }

    private Vector3 _desiredPosition;

    public void SetPlayerEntity(Player player)
    {
        ControlledPlayer = player;
        CameraController.Anchor = player;

        _desiredPosition = ControlledPlayer.Position;
    }

    public override void _Ready()
    {
        Instance = this;
        CameraController.OnLeftClick += clickedLocation =>
        {
            _desiredPosition = clickedLocation;
        };
    }

    public override void _PhysicsProcess(double delta)
    {
        if (ControlledPlayer is null)
        {
            return;
        }

        ControlledPlayer.Position = ControlledPlayer.Position.MoveToward(
            _desiredPosition,
            (float)(MoveSpeed * delta)
        );
    }
}
