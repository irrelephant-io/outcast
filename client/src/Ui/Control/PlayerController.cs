using Godot;
using Irrelephant.Outcast.Client.Networking;
using Irrelephant.Outcast.Client.Networking.Extensions;
using Irrelephant.Outcast.Client.Ui.Camera;
using Irrelephant.Outcast.Client.Ui.Interface;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

namespace Irrelephant.Outcast.Client.Ui.Control;

public partial class PlayerController : Node
{
    public static PlayerController Instance { get; private set; } = null!;

    public Entities.PlayerEntity? ControlledPlayer { get; set; }

    [Export]
    public CameraController CameraController = null!;

    public void SetPlayerEntity(Entities.PlayerEntity playerEntity)
    {
        ControlledPlayer = playerEntity;
        CameraController.Anchor = playerEntity;
    }

    public override void _ExitTree()
    {
        if (ControlledPlayer is not null)
        {
            var client = NetworkService.Instance.Client!;
            client.EnqueueOutboundMessage(new DisconnectNotification(client.SessionId, "Exiting."));
        }
    }

    public override void _Ready()
    {
        Instance = this;
        CameraController.OnLeftClick += clickedLocation =>
        {
            NetworkService.Instance.Client!.EnqueueOutboundMessage(
                new MoveCommand(
                    clickedLocation.ToClrVector()
                )
            );
        };
    }

    public void AttackCurrentTarget()
    {
        if (UiController.Instance.TargetedEntity is {} target)
        {
            NetworkService.Instance.Client!.EnqueueOutboundMessage(
                new AttackCommand(target.RemoteId)
            );
        }
    }
}
