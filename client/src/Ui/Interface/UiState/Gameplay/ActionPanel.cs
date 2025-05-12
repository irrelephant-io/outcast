using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Irrelephant.Outcast.Client.Ui.Interface.UiState.Gameplay;

public partial class ActionPanel : PanelContainer
{
    public int ActionPanelIndex { get; } = 1;

    private ActionPanelSlot[] _actionPanelSlots = null!;
    private HBoxContainer _actionPanelContainer = null!;

    private readonly IDictionary<Key, ActionPanelSlot> _actionPanelSlotMap
        = new Dictionary<Key, ActionPanelSlot>();

    public override void _EnterTree()
    {
        _actionPanelContainer = GetNode<HBoxContainer>("MarginContainer/HBoxContainer");
        _actionPanelSlots = _actionPanelContainer.GetChildren().OfType<ActionPanelSlot>().ToArray();

    }

    public override void _Ready()
    {
        var controlledActions = InputMap.GetActions()
            .Select(it => it.ToString())
            .Where(it => it.StartsWith($"ActionPanel_{ActionPanelIndex}")).ToArray();

        foreach (var (panelSlot, idx) in _actionPanelSlots.Select((it, idx) => (it, idx)))
        {
            var events = InputMap.ActionGetEvents(controlledActions[idx]);
            var key = events.OfType<InputEventKey>().First().PhysicalKeycode;
            panelSlot.SetLabel(key.ToString());
            _actionPanelSlotMap[key] = panelSlot;
        }

        base._Ready();
    }

    public override void _Input(InputEvent evt)
    {
        if (evt is InputEventKey { Pressed: true } eventKey)
        {
            if (_actionPanelSlotMap.TryGetValue(eventKey.PhysicalKeycode, out var actionPanelSlot))
            {
                actionPanelSlot.Activate();
            }
        }
    }
}
