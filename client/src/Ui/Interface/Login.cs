using Godot;
using Irrelephant.Outcast.Client.Networking;

namespace Irrelephant.Outcast.Client.Ui.Interface;

public partial class Login : Godot.Control
{
    [Export] public NetworkService NetworkService { get; set; } = null!;

    private TextEdit _userNameInput = null!;

    private Button _connectButton = null!;

    public override void _Ready()
    {
        _userNameInput = GetNode<TextEdit>("Layout/UserName");
        _connectButton = GetNode<Button>("Layout/ConnectButton");

        var config = new ConfigFile();
        if (config.Load("user://settings.cfg") == Error.Ok)
        {
            _userNameInput.Text = (string)config.GetValue("UserSettings", "PreferredUserName");
        }

        _connectButton.Pressed += () =>
        {
            config.SetValue("UserSettings", "PreferredUserName", _userNameInput.Text);
            config.Save("user://settings.cfg");

            _connectButton.Disabled = false;
            NetworkService.InitiateConnectAndLogin(_userNameInput.Text);
            Visible = false;
        };

        base._Ready();
    }
}
