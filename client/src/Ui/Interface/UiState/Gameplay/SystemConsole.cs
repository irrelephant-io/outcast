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

        Callable.From(
            () => _consoleScrollContainer.SetVScroll((int)_consoleScrollContainer.GetVScrollBar().MaxValue)
        ).CallDeferred();
    }
}
