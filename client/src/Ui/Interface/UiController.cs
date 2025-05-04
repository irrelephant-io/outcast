using Godot;
using Irrelephant.Outcast.Client.Ui.Camera;

namespace Irrelephant.Outcast.Client.Ui.Interface;

public partial class UiController : Node
{
    public static UiController Instance { get; private set; }

    [Export]
    public RichTextLabel Databox { get; set; }

    [Export]
    public CameraController CameraController { get; set; }

    public override void _Ready()
    {
        Instance = this;
        CameraController.OnEntityClick += clickedEntity =>
        {
            Databox.Text = $"Selected: {clickedEntity.RemoteId}";
        };
    }

    public void FinishConnect()
    {
        Databox.Visible = true;
    }

}
