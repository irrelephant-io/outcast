using Godot;
using Irrelephant.Outcast.Client.Entities;
using Irrelephant.Outcast.Client.Ui.Camera;
using Irrelephant.Outcast.Client.Ui.Interface.UiState;

namespace Irrelephant.Outcast.Client.Ui.Interface;

public partial class UiController : Node
{
    public static UiController Instance { get; private set; } = null!;

    [Export] public CameraController CameraController { get; set; } = null!;

    public NetworkedEntity? TargetedEntity { get; set; }

    [Export]
    public UiStateController UiStateController = null!;

    [Signal]
    public delegate void TargetUpdatedEventHandler(NetworkedEntity? entity);

    public override void _Ready()
    {
        Instance = this;
        CameraController.OnEntityClick += SetTarget;
    }

    public void SetTarget(NetworkedEntity? target)
    {
        TargetedEntity = target;
        EmitSignalTargetUpdated(TargetedEntity);
    }

    public void FinishConnect()
    {
        UiStateController.GoToState(UiStates.Gameplay);
    }

}
