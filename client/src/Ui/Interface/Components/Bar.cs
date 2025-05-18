using Godot;

namespace Irrelephant.Outcast.Client.Ui.Interface.Components;

public partial class Bar : Container
{
    [Export] public Color Color { get; set; }

    [Export] public string? Label { get; set; }

    private float _maxValue = 100f;
    [Export]
    public float MaxValue
    {
        get => _maxValue;
        set
        {
            _maxValue = value;
            if (_maxValue > 0 && Visible) Redraw();
        }
    }

    private float _currentValue;
    [Export]
    public float CurrentValue
    {
        get => _currentValue;
        set
        {
            _currentValue = value;
            if (_maxValue > 0 && Visible) Redraw();
        }
    }

    [Export] public bool ShowText { get; set; } = true;

    private bool _isReady = false;
    private Label _label = null!;
    private Label _valueText = null!;
    private TextureRect _barRect = null!;
    private TextureRect _barShadowRect = null!;

    public override void _EnterTree()
    {
        _label = GetNode<Label>("Label");
        _valueText = GetNode<Label>("ValueText");
        _barRect = GetNode<TextureRect>("BarFill");
        _barShadowRect = GetNode<TextureRect>("BarShadow");

        _label.Text = Label;
        _barRect.SetModulate(Color);

        _isReady = true;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized || what == NotificationVisibilityChanged)
        {
            Redraw();
        }
    }

    private void Redraw()
    {
        if (!Visible || !_isReady)
        {
            return;
        }

        if (ShowText)
        {
            _valueText.Text = $"{CurrentValue:N0}/{MaxValue:N0}";
            _label.Position = new Vector2((Size.X - _label.Size.X) * 0.1f, (Size.Y - _label.Size.Y) * 0.5f);
            _valueText.Position = new Vector2((Size.X - _valueText.Size.X) * 0.5f, (Size.Y - _valueText.Size.Y) * 0.5f);
        }
        else
        {
            _valueText.Visible = false;
            _label.Visible = false;
        }

        var sizeX = Mathf.Round(Size.X * Mathf.Min(1f, CurrentValue / MaxValue));
        _barRect.Size = new Vector2(sizeX, Size.Y);
        _barRect.Visible = sizeX > 0;
        _barShadowRect.Size = new Vector2(sizeX, _barShadowRect.Size.Y);
        _barShadowRect.Visible = sizeX > 0;
    }
}
