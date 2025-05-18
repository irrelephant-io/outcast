using System;
using Godot;

namespace Irrelephant.Outcast.Client.Ui.Interface.UiState.Gameplay;

public partial class SystemConsole : PanelContainer
{
    private int _currentLines = 0;

    public static SystemConsole? Instance { get; private set; }

    [Export]
    public int MaxLines { get; set; }

    [Export] public PackedScene LogEntryTemplate { get; set; } = null!;

    private VBoxContainer _consoleContainer = null!;

    private ScrollContainer _consoleScrollContainer = null!;

    public override void _EnterTree()
    {
        Instance = this;
        _consoleContainer = GetNode<VBoxContainer>("MarginContainer/ScrollContainer/Messages");
        _consoleScrollContainer = GetNode<ScrollContainer>("MarginContainer/ScrollContainer");
        AddMessage("Welcome to Outcast Online!");
    }

    public override void _ExitTree()
    {
        Instance = null;
    }

    public void AddMessage(string message)
    {
        var logEntry = LogEntryTemplate.Instantiate<RichTextLabel>();
        logEntry.Text = message;
        _consoleContainer.AddChild(logEntry);
        _currentLines++;
        if (_currentLines > MaxLines)
        {
            var firstMessage = _consoleContainer.GetChild<RichTextLabel>(0);
            _consoleContainer.RemoveChild(firstMessage);
            firstMessage.QueueFree();
        }

        // This is a workaround to a weird scrolling issue. Have to defer by 2 frames because of
        // VScroll weirdness.
        Callable.From(ScheduleScrollToBottomFrame1).CallDeferred();
    }

    private void ScheduleScrollToBottomFrame1()
    {
        Callable.From(ScheduleScrollToBottomFrame2).CallDeferred();
    }

    private void ScheduleScrollToBottomFrame2()
    {
        _consoleScrollContainer.SetVScroll(int.MaxValue);
    }

}
