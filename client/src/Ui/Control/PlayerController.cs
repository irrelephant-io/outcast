using Godot;
using Irrelephant.Outcast.Client.Networking.Extensions;
using Irrelephant.Outcast.Client.Ui.Camera;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

namespace Irrelephant.Outcast.Client.Ui.Control;

public partial class PlayerController : Node
{
    public static PlayerController Instance { get; private set; }

    public Entities.PlayerEntity? ControlledPlayer { get; set; }

    [Export]
    public CameraController CameraController = null!;

    [Export]
    public float MoveSpeed { get; set; }

    public void SetPlayerEntity(Entities.PlayerEntity playerEntity)
    {
        ControlledPlayer = playerEntity;
        CameraController.Anchor = playerEntity;
    }

    public override void _ExitTree()
    {
        if (ControlledPlayer is not null)
        {
            var client = ControlledPlayer.OwningClient;
            client.EnqueueOutboundMessage(new DisconnectNotice(client.SessionId, "Exiting."));
        }
    }

    public override void _Ready()
    {
        Instance = this;
        CameraController.OnLeftClick += clickedLocation =>
        {
            ControlledPlayer?.OwningClient.EnqueueOutboundMessage(
                new InitiateMoveRequest(
                    clickedLocation.ToClrVector()
                )
            );
        };
    }
}
