using Godot;
using Irrelephant.Outcast.Client.Networking;

namespace Irrelephant.Outcast.Client.Ui.Interface.UiState.Login;

public partial class ConnectMenu : Godot.Control
{
    private LineEdit _userNameInput = null!;

    private LineEdit _passwordInput = null!;

    private Button _connectButton = null!;

    public override void _Ready()
    {
        _userNameInput = GetNode<LineEdit>("PanelContainer/MarginContainer/Layout/UserName");
        _passwordInput = GetNode<LineEdit>("PanelContainer/MarginContainer/Layout/Password");
        _connectButton = GetNode<Button>("PanelContainer/MarginContainer/Layout/ConnectButton");

        var config = new ConfigFile();
        if (config.Load("user://settings.cfg") == Error.Ok)
        {
            _userNameInput.Text = (string)config.GetValue("UserSettings", "PreferredUserName");
            _passwordInput.GrabFocus();
        }
        else
        {
            _userNameInput.GrabFocus();
        }

        _connectButton.Pressed += () => DoLogin(config);
        _passwordInput.TextSubmitted += _ => DoLogin(config);
        _userNameInput.TextSubmitted += _ => DoLogin(config);

        base._Ready();
    }

    private void DoLogin(ConfigFile config)
    {
        config.SetValue("UserSettings", "PreferredUserName", _userNameInput.Text);
        config.Save("user://settings.cfg");

        _userNameInput.Editable = false;
        _passwordInput.Editable = false;
        _connectButton.Disabled = false;
        NetworkService.Instance.InitiateConnectAndLogin(_userNameInput.Text);
        UiController.Instance.FinishConnect();
    }
}
