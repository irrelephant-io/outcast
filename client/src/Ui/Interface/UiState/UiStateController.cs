using Godot;

namespace Irrelephant.Outcast.Client.Ui.Interface.UiState;

public enum UiStates
{
    Login,
    Gameplay
}

public partial class UiStateController : Godot.Control
{
    [Export] public PackedScene LoginUiState { get; set; } = null!;

    [Export] public PackedScene GameplayUiState { get; set; } = null!;


    private Node? _currentState;

    public override void _Ready()
    {
        GoToState(UiStates.Login);
    }

    public void GoToState(UiStates state)
    {
        if (state == UiStates.Login)
        {
            UpdateStateNodeWithScene(LoginUiState);
        } else if (state == UiStates.Gameplay)
        {
            UpdateStateNodeWithScene(GameplayUiState);
        }
    }

    private void UpdateStateNodeWithScene(PackedScene scene)
    {
        _currentState?.QueueFree();
        var stateNode = scene.Instantiate();
        Callable.From(() => AddChild(stateNode)).CallDeferred();
        _currentState = stateNode;
    }
}
