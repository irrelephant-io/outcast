using Godot;

namespace Irrelephant.Outcast.Client.Ui.Interface.UiState.Login;

public partial class LoginUiState : Node
{
    public UiStateController StateController = null!;

    public override void _EnterTree()
    {
        StateController = (UiStateController)FindParent("UserInterface");
    }

    public override void _ExitTree()
    {

    }
}
