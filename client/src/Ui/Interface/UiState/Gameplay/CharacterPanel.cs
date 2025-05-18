using Godot;
using Irrelephant.Outcast.Client.Ui.Control;
using Irrelephant.Outcast.Client.Ui.Interface.Components;

namespace Irrelephant.Outcast.Client.Ui.Interface.UiState.Gameplay;

public partial class CharacterPanel : PanelContainer
{
    private Bar _hpBar = null!;
    private Label _characterName = null!;

    public override void _EnterTree()
    {
        _hpBar = GetNode<Bar>("MarginContainer/StatsContainer/HP");
        _characterName = GetNode<Label>("MarginContainer/StatsContainer/CharacterNameLabel");
    }

    public override void _Ready()
    {
        PlayerController.Instance.ControlledPlayer!.OnCurrentHealthUpdated += UpdateCurrentHealth;
        PlayerController.Instance.ControlledPlayer!.OnMaxHealthUpdated += UpdateMaxHealth;
        _hpBar.CurrentValue = PlayerController.Instance.ControlledPlayer.CurrentHealth!.Value;
        _hpBar.MaxValue = PlayerController.Instance.ControlledPlayer.MaxHealth!.Value;
        _characterName.Text = PlayerController.Instance.ControlledPlayer.EntityName;
    }

    private void UpdateMaxHealth(int maxHealth)
    {
        _hpBar.MaxValue = maxHealth;
    }

    private void UpdateCurrentHealth(int currentHealth)
    {
        _hpBar.CurrentValue = currentHealth;
    }
}
