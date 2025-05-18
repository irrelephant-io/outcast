using Godot;
using Irrelephant.Outcast.Client.Entities;
using Irrelephant.Outcast.Client.Ui.Interface.Components;

namespace Irrelephant.Outcast.Client.Ui.Interface.UiState.Gameplay;

public partial class CurrentTarget : PanelContainer
{
    private Label _label = null!;
    private Button _deselectButton = null!;
    private Bar _currentHealth = null!;
    private NetworkedEntity? _currentTarget;

    public override void _EnterTree()
    {
        _label = GetNode<Label>("MarginContainer/SelectionName");
        _deselectButton = GetNode<Button>("MarginContainer/DeselectButton");
        _currentHealth = GetNode<Bar>("MarginContainer/CurrentHealth");

        base._EnterTree();
    }

    public override void _Ready()
    {
        UiController.Instance.TargetUpdated += target =>
        {
            if (_currentTarget is not null)
            {
                _currentTarget.OnCurrentHealthUpdated -= UpdateCurrentHealthValue;
                _currentTarget.OnMaxHealthUpdated -= UpdateMaxHealthValue;
            }
            _currentTarget = target;
            if (target is not null)
            {
                _label.Text = target.EntityName;
                target.OnCurrentHealthUpdated += UpdateCurrentHealthValue;
                target.OnMaxHealthUpdated += UpdateMaxHealthValue;
                if (target.CurrentHealth.HasValue)
                {
                    UpdateCurrentHealthValue(target.CurrentHealth.Value);
                }
                if (target.MaxHealth.HasValue)
                {
                    UpdateMaxHealthValue(target.MaxHealth.Value);
                }
            }
            Visible = target is not null;
        };

        _deselectButton.Pressed += () =>
        {
            UiController.Instance.SetTarget(null);
        };
        base._Ready();
    }

    private void UpdateCurrentHealthValue(int newValue)
    {
        _currentHealth.CurrentValue = newValue;
        if (_currentTarget?.MaxHealth.HasValue is true)
        {
            _currentHealth.MaxValue = _currentTarget.MaxHealth.Value;
            _currentHealth.Visible = true;
        }
        else
        {
            _currentHealth.Visible = false;
        }
    }

    private void UpdateMaxHealthValue(int newValue)
    {
        _currentHealth.MaxValue = newValue;
        if (_currentTarget?.CurrentHealth.HasValue is true)
        {
            _currentHealth.CurrentValue = _currentTarget.CurrentHealth.Value;
            _currentHealth.Visible = true;
        }
        else
        {
            _currentHealth.Visible = false;
        }
    }
}
