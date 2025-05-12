using Godot;

namespace Irrelephant.Outcast.Client.Ui.Interface.UiState.Gameplay;

public partial class CurrentTarget : PanelContainer
{
    private Label _label = null!;
    private Button _deselectButton = null!;

    public override void _EnterTree()
    {
        _label = GetNode<Label>("MarginContainer/SelectionName");
        _deselectButton = GetNode<Button>("MarginContainer/DeselectButton");

        base._EnterTree();
    }

    public override void _Ready()
    {
        UiController.Instance.TargetUpdated += target =>
        {
            if (target is not null)
            {
                _label.Text = target.EntityName;
            }
            Visible = target is not null;
        };

        _deselectButton.Pressed += () =>
        {
            UiController.Instance.SetTarget(null);
        };
        base._Ready();
    }
}
