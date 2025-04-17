using System.Net.Sockets;
using Godot;

public partial class NetworkManager : Node
{

    public Socket _connectSocket = new Socket(
        AddressFamily.InterNetworkV6,
        SocketType.Stream,
        ProtocolType.Tcp
    )
    {

    };

    public override void _Ready()
    {


        base._Ready();
    }
}
