using Godot;
using Irrelephant.Outcast.Client.Networking.Extensions;
using Irrelephant.Outcast.Client.Ui.Camera;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

namespace Irrelephant.Outcast.Client.Ui.Control;

public partial class PlayerController : Node
{
    public static PlayerController Instance { get; private set; }

    public Player? ControlledPlayer { get; set; }

    [Export]
    public CameraController CameraController = null!;

    [Export]
    public float MoveSpeed { get; set; }

    public void SetPlayerEntity(Player player)
    {
        ControlledPlayer = player;
        CameraController.Anchor = player;
    }

    public override void _Ready()
    {
        Instance = this;
        CameraController.OnLeftClick += clickedLocation =>
        {
            ControlledPlayer?.OwningClient.EnqueueOutboundMessage(
                new InitiateMoveCommand(
                    clickedLocation.ToClrVector()
                )
            );
        };
    }
}
