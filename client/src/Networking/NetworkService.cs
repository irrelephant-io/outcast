using System.Net;
using System.Net.Sockets;
using Godot;

public partial class NetworkService : Node
{
    private const string ServerHostName = "dev.outcast.irrelephant.io";
    private const int ServerPort = 42069;

    private byte[] _ioBuffer = new byte[1024];

    public Socket _connectSocket = new(
        AddressFamily.InterNetworkV6,
        SocketType.Stream,
        ProtocolType.Tcp
    )
    {
        Blocking = false
    };

    public override void _Ready()
    {
        var entry = Dns.GetHostEntry(ServerHostName);
        var serverAddress = new IPEndPoint(entry.AddressList[0], ServerPort);
        var localhostEntry = Dns.GetHostEntry(IPAddress.IPv6Loopback);
        var localEndPoint = new IPEndPoint(localhostEntry.AddressList[0], 0);

        var evtArgs = new SocketAsyncEventArgs
        {
            RemoteEndPoint = serverAddress
        };
        evtArgs.Completed += (sender, args) =>
        {
            if (args.LastOperation == SocketAsyncOperation.Connect)
            {
                ProcessConnect(args);
            }
        };
        evtArgs.SetBuffer(_ioBuffer, 0, _ioBuffer.Length);
        _connectSocket.Bind(localEndPoint);
        evtArgs.UserToken = 1;
        var isAsync = _connectSocket.ConnectAsync(evtArgs);
        if (!isAsync)
        {
            ProcessConnect(evtArgs);
        }

        base._Ready();
    }

    private void ProcessConnect(SocketAsyncEventArgs evtArgs)
    {
        var attempt = (int)evtArgs.UserToken!;
        if (evtArgs.SocketError != SocketError.Success)
        {
            if (attempt >= 5)
            {
                GD.PrintErr("Unable to connect to server [5/5] ... Giving up.");
                return;
            }

            GD.PrintErr($"Unable to connect to server [{attempt}/5] ... Trying again...");
            evtArgs.UserToken = attempt + 1;
            var isAsync = _connectSocket.ConnectAsync(evtArgs);
            if (!isAsync)
            {
                ProcessConnect(evtArgs);
            }
        }
        else
        {
            GD.Print("Socket connected...");
        }
    }

    public override void _ExitTree()
    {
        _connectSocket.Close();
        base._ExitTree();
    }
}
