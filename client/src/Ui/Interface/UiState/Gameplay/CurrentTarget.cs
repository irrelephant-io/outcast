using System;
using Godot;
using Irrelephant.Outcast.Client.Entities;

namespace Irrelephant.Outcast.Client.Ui.Interface.UiState.Gameplay;

public partial class CurrentTarget : PanelContainer
{
    private Label _label = null!;
    private Button _deselectButton = null!;
    private ProgressBar _currentHealth = null!;
    private NetworkedEntity? _currentTarget;

    public override void _EnterTree()
    {
        _label = GetNode<Label>("MarginContainer/SelectionName");
        _deselectButton = GetNode<Button>("MarginContainer/DeselectButton");
        _currentHealth = GetNode<ProgressBar>("MarginContainer/CurrentHealth");

        base._EnterTree();
    }

    public override void _Ready()
    {
        UiController.Instance.TargetUpdated += target =>
        {
            _currentTarget = target;
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

    public override void _Process(double delta)
    {
        if (_currentTarget?.HealthPercentage.HasValue ?? false)
        {
            _currentHealth.Value = _currentTarget.HealthPercentage.Value;
        }
        else
        {
            _currentHealth.Visible = false;
        }
        base._Process(delta);
    }
}
