using System.Net.Sockets;
using Irrelephant.Outcast.Protocol.Encoding;
using Irrelephant.Outcast.Server.Configuration;
using Irrelephant.Outcast.Server.Protocol;
using Microsoft.Extensions.Options;

namespace Irrelephant.Outcast.Server.Networking;

public class NetworkIoSocketPool : IAsyncDisposable
{
    /// <summary>
    /// Controls simultaneous access to critical resources.
    /// </summary>
    private readonly Lock _concurrencyLock = new();
    private readonly IList<SocketAsyncEventArgs> _controlledSockets;
    private readonly Stack<SocketAsyncEventArgs> _availableSockets;

    public NetworkIoSocketPool(IOptions<OutcastNetworkingOptions> options)
    {
        _controlledSockets = Enumerable.Range(0, options.Value.MaxSimultaneousConnectionRequests)
            .Select(_ => new SocketAsyncEventArgs
            {
                UserToken = new SocketProtocolState(options)
            })
            .ToList();

        _availableSockets = new Stack<SocketAsyncEventArgs>(_controlledSockets);
    }

    public SocketAsyncEventArgs Borrow()
    {
        lock (_concurrencyLock)
        {
            return _availableSockets.Pop();
        }
    }

    public void Release(SocketAsyncEventArgs socketAsyncEventArgs)
    {
        lock (_concurrencyLock)
        {
            socketAsyncEventArgs.AcceptSocket?.Close();
            socketAsyncEventArgs.AcceptSocket = null;
            if (socketAsyncEventArgs.UserToken is SocketProtocolState state)
            {
                state.Reset();
            }
            _availableSockets.Push(socketAsyncEventArgs);
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
