using System.Net;
using System.Net.Sockets;
using Godot;
using Irrelephant.Outcast.Networking.Protocol;
using Irrelephant.Outcast.Networking.Transport;

namespace Irrelephant.Outcast.Client.Networking;

public partial class NetworkService : Node
{
    public static NetworkService Instance { get; private set; } = null!;

    private const string ServerHostName = "dev.outcast.irrelephant.io";
    private const int ServerPort = 42069;

    private byte[] _ioBuffer = new byte[1024];
    public Client? Client { get; private set; }

    private SocketAsyncEventArgs _socketAsyncEventArgs = null!;

    public void InitiateConnectAndLogin(string playerName)
    {
        var entry = Dns.GetHostEntry(ServerHostName);
        var serverAddress = new IPEndPoint(entry.AddressList[0], ServerPort);

        _socketAsyncEventArgs = new SocketAsyncEventArgs
        {
            RemoteEndPoint = serverAddress
        };
        _socketAsyncEventArgs.UserToken = (playerName, 0);
        _socketAsyncEventArgs.Completed += ProcessConnect;

        var isAsync = Socket.ConnectAsync(SocketType.Stream, ProtocolType.Tcp, _socketAsyncEventArgs);
        if (!isAsync)
        {
            ProcessConnect(_socketAsyncEventArgs);
        }
    }

    public override void _Ready()
    {
        Instance = this;
    }

    private void ProcessConnect(object? sender, SocketAsyncEventArgs e) =>
        ProcessConnect(e);

    private void ProcessConnect(SocketAsyncEventArgs evtArgs)
    {
        var (playerName, attempt) = ((string, int))evtArgs.UserToken!;
        if (evtArgs.SocketError != SocketError.Success)
        {
            if (evtArgs.SocketError == SocketError.ConnectionRefused)
            {
                GD.PrintErr($"Unable to connect to server ... Actively refused.");
                return;
            }

            if (attempt >= 5)
            {
                GD.PrintErr("Unable to connect to server [5/5] ... Giving up.");
                return;
            }

            GD.PrintErr($"Unable to connect to server [{attempt}/5] ... Trying again...");
            evtArgs.UserToken = (playerName, attempt + 1);
            var isAsync = evtArgs.ConnectSocket!.ConnectAsync(evtArgs);
            if (!isAsync)
            {
                ProcessConnect(evtArgs);
            }
        }
        else
        {
            evtArgs.Completed -= ProcessConnect;
            Client = new Client(
                TcpTransportHandler.FromSocketAsyncEventArgs(evtArgs),
                new JsonMessageCodec(),
                playerName
            );
            Client.Initialize();
            GD.Print("Socket connected...");
        }
    }

    public override void _ExitTree()
    {
        Client?.Dispose();
        Client = null!;
        base._ExitTree();
    }
}
