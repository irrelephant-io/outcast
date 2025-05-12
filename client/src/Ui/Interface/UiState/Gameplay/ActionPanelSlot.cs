using Godot;
using Irrelephant.Outcast.Client.Ui.Control;

namespace Irrelephant.Outcast.Client.Ui.Interface.UiState.Gameplay;

public partial class ActionPanelSlot : TextureRect
{

    private Label _label = null!;
    private TextureButton _button = null!;

    public override void _EnterTree()
    {
        _label = GetChild<Label>(0);
        _button = GetChild<TextureButton>(1);

        _button.Pressed += Activate;

        base._EnterTree();
    }

    public void SetLabel(string label)
    {
        _label.Text = label;
    }

    public void Activate()
    {
        PlayerController.Instance.AttackCurrentTarget();
        GD.Print("Activated " + _label.Text);
    }
}
