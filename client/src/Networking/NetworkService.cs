using System.Net;
using System.Net.Sockets;
using Godot;
using Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Encoding;
using Irrelephant.Outcast.Protocol.Networking;
using Irrelephant.Outcast.Protocol.Networking.EventModel;
using Irrelephant.Outcast.Protocol.Networking.Session;
using Microsoft.Extensions.Options;

namespace Irrelephant.Outcast.Client.Networking;

public partial class NetworkService : Node
{
    private const string ServerHostName = "dev.outcast.irrelephant.io";
    private const int ServerPort = 42069;

    private byte[] _ioBuffer = new byte[1024];

    private IClient _client = null!;

    private readonly Socket _socket = new(
        AddressFamily.InterNetworkV6,
        SocketType.Stream,
        ProtocolType.Tcp
    )
    {
        Blocking = false
    };

    private SocketAsyncEventArgs _socketAsyncEventArgs = null!;

    public void InitiateConnectAndLogin(string playerName)
    {
        var localhostEntry = Dns.GetHostEntry(IPAddress.IPv6Loopback);
        var localEndPoint = new IPEndPoint(localhostEntry.AddressList[0], 0);
        var entry = Dns.GetHostEntry(ServerHostName);
        var serverAddress = new IPEndPoint(entry.AddressList[0], ServerPort);

        _socketAsyncEventArgs = new SocketAsyncEventArgs();
        _socketAsyncEventArgs.RemoteEndPoint = serverAddress;

        var messageHandler = new IoSocketMessageHandler(
            _socketAsyncEventArgs,
            Options.Create(
                new NetworkingOptions
                {
                    MessageCodec = new JsonMessageCodec()
                }
            )
        );
        _client = new Client
        {
            ClientName = playerName,
            MessageHandler = messageHandler,
            ProtocolHandler = new ClientSideProtocolHandler(messageHandler, playerName)
        };


        messageHandler.OutboundMessageAvailable += OnOutboundAvailable;
        _socketAsyncEventArgs.Completed += OnIoOperationCompleted;

        _socket.Bind(localEndPoint);
        _socketAsyncEventArgs.UserToken = 1;
        var isAsync = _socket.ConnectAsync(_socketAsyncEventArgs);
        _socketAsyncEventArgs.SetBuffer(_ioBuffer, 0, _ioBuffer.Length);
        if (!isAsync)
        {
            ProcessConnect(_socketAsyncEventArgs);
        }
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
            var isAsync = _socket.ConnectAsync(evtArgs);
            if (!isAsync)
            {
                ProcessConnect(evtArgs);
            }
        }
        else
        {
            _client.ProtocolHandler.HandleAfterTransportConnected();
            _socket.ReceiveAsync(evtArgs);
            GD.Print("Socket connected...");
        }
    }

    private void OnIoOperationCompleted(object? sender, SocketAsyncEventArgs e)
    {
        if (
            e.LastOperation == SocketAsyncOperation.Disconnect
            || e is { LastOperation: SocketAsyncOperation.Receive, BytesTransferred: 0 }
        )
        {
            ProcessDisconnect(e);
        }
        else if (e.LastOperation == SocketAsyncOperation.Receive)
        {
            ProcessReceive(e, isAsync: true);
        } else if (e.LastOperation == SocketAsyncOperation.Connect)
        {
            ProcessConnect(e);
        }
    }

    private void ProcessDisconnect(SocketAsyncEventArgs e)
    {
        _client.ProtocolHandler.HandleBeforeTransportDisconnected();
        e.ConnectSocket!.Shutdown(SocketShutdown.Both);
        e.ConnectSocket!.Close();
        e.Completed -= OnIoOperationCompleted;
    }

    private void ProcessReceive(SocketAsyncEventArgs eventArgs, bool isAsync)
    {
        if (eventArgs.BytesTransferred > 0 && eventArgs.SocketError == SocketError.Success)
        {
            _client.MessageHandler.ProcessRead(eventArgs.MemoryBuffer, eventArgs.BytesTransferred, isAsync);
            var isAsyncPending = eventArgs.ConnectSocket!.ReceiveAsync(eventArgs);
            if (!isAsyncPending)
            {
                ProcessReceive(eventArgs, false);
            }
        }
        else
        {
            ProcessDisconnect(eventArgs);
        }
    }

    private void OnOutboundAvailable(object? sender, OutboundMessageAvailableArgs args)
    {
        args.MessageContents.CopyTo(args.Socket.MemoryBuffer);
        args.Socket.SetBuffer(args.Socket.Offset, args.MessageContents.Length);
        args.Socket.ConnectSocket!.SendAsync(args.Socket);
    }

    public override void _ExitTree()
    {
        _socket.Close();
        base._ExitTree();
    }
}
