using System.Net.Sockets;
using Irrelephant.Outcast.Protocol.Networking;
using Irrelephant.Outcast.Server.Configuration;
using Irrelephant.Outcast.Server.Protocol.Client;
using Irrelephant.Outcast.Server.Utility;
using Microsoft.Extensions.Options;

namespace Irrelephant.Outcast.Server.Networking;

public class IoSocketPool : IAsyncDisposable
{
    private readonly IOptions<ServerNetworkingOptions> _options;

    /// <summary>
    /// Controls simultaneous access to critical resources.
    /// </summary>
    private readonly ReaderWriterLockSlim _rwLock = new(LockRecursionPolicy.SupportsRecursion);

    private readonly IList<SocketAsyncEventArgs> _controlledSockets;
    private readonly Stack<SocketAsyncEventArgs> _availableSockets;
    private readonly ThreadSafeList<SocketAsyncEventArgs> _activeSockets;

    public IEnumerable<SocketAsyncEventArgs> ActiveSockets => _activeSockets;

    public IoSocketPool(IOptions<ServerNetworkingOptions> options)
    {
        _options = options;
        _controlledSockets = Enumerable.Range(0, options.Value.MaxSimultaneousConnectionRequests)
            .Select(_ =>
            {
                var socket = new SocketAsyncEventArgs();
                socket.UserToken = new IoSocketMessageHandler(socket, options);
                return socket;
            })
            .ToList();
        _activeSockets = new([], _rwLock);
        _availableSockets = new Stack<SocketAsyncEventArgs>(_controlledSockets);
    }

    public SocketAsyncEventArgs BorrowForNewClient()
    {
        using (_rwLock.LockForWrite())
        {
            var borrowedSocket = _availableSockets.Pop();
            var borrowedMessageQueue = (IoSocketMessageHandler)borrowedSocket.UserToken!;
            borrowedSocket.UserToken = new ServerSideConnectingClient(_options, borrowedMessageQueue);
            _activeSockets.Add(borrowedSocket);
            return borrowedSocket;
        }
    }

    public void Release(SocketAsyncEventArgs socketAsyncEventArgs)
    {
        using (_rwLock.LockForWrite())
        {
            socketAsyncEventArgs.AcceptSocket?.Close();
            socketAsyncEventArgs.AcceptSocket = null;
            if (socketAsyncEventArgs.UserToken is ServerSideConnectingClient client)
            {
                var messageQueue = client.MessageHandler;
                messageQueue.Reset();
                socketAsyncEventArgs.UserToken = messageQueue;
                client.Dispose();
            }
            _availableSockets.Push(socketAsyncEventArgs);
            _activeSockets.Remove(socketAsyncEventArgs);
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var socket in _controlledSockets)
        {
            if (socket.AcceptSocket?.Connected is true)
            {
                await socket.AcceptSocket.DisconnectAsync(reuseSocket: false);
            }
            socket.Dispose();
        }
    }
}
