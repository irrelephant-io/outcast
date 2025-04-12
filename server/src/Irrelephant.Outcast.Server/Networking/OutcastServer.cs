using System.Net.Sockets;
using Irrelephant.Outcast.Server.Configuration;
using Irrelephant.Outcast.Server.Protocol;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Irrelephant.Outcast.Server.Networking;

public class OutcastServer(
    IOptions<OutcastNetworkingOptions> options,
    ILogger<OutcastServer> logger
) : IAsyncDisposable
{
    private readonly TaskCompletionSource _completionSource = new();

    private readonly Socket _listenSocket = new(
        AddressFamily.InterNetworkV6,
        SocketType.Stream,
        ProtocolType.Tcp
    )
    {
        Blocking = false
    };

    private readonly NetworkIoBufferPool _bufferPool = new(options);

    private readonly NetworkIoSocketPool _socketPool = new(options);

    public Task RunAsync(CancellationToken cancellationToken)
    {
        var configuration = options.Value;
        _listenSocket.Bind(configuration.ConnectionListenEndpoint);
        _listenSocket.Listen(configuration.MaxConnectionBacklog);

        logger.LogInformation("Bound and listening on {EndPoint}.", configuration.ConnectionListenEndpoint);

        var acceptArgs = new SocketAsyncEventArgs();
        acceptArgs.Completed += OnAcceptCompleted;
        StartAccept(acceptArgs);

        cancellationToken.Register(() => _completionSource.TrySetCanceled(cancellationToken));
        return _completionSource.Task;
    }

    private void StartAccept(SocketAsyncEventArgs acceptArgs)
    {
        acceptArgs.AcceptSocket = null;
        var isAsyncPending = _listenSocket.AcceptAsync(acceptArgs);
        if (!isAsyncPending)
        {
            ProcessAccept(acceptArgs);
        }
    }

    private void OnAcceptCompleted(object? sender, SocketAsyncEventArgs e)
    {
        ProcessAccept(e);
        StartAccept(e);
    }

    private void ProcessAccept(SocketAsyncEventArgs eventArgs)
    {
        logger.LogInformation("Incoming connection from {RemoteEndPoint}.", eventArgs.RemoteEndPoint);
        var ioSocket = _socketPool.Borrow();
        ioSocket.Completed += OnIoOperationCompleted;
        ioSocket.AcceptSocket = eventArgs.AcceptSocket;

        _bufferPool.Assign(ioSocket);
        var isAsyncPending = ioSocket.AcceptSocket!.ReceiveAsync(ioSocket);
        if (!isAsyncPending)
        {
            ProcessReceive(ioSocket, isAsyncPending);
        }
    }

    private void ProcessReceive(SocketAsyncEventArgs eventArgs, bool isAsync)
    {
        logger.LogInformation("Incoming data from {RemoteEndPoint}.", eventArgs.RemoteEndPoint);
        var protocolState = (SocketProtocolState)eventArgs.UserToken!;
        protocolState.ProcessRead(eventArgs.MemoryBuffer, eventArgs.BytesTransferred, isAsync);
        var isAsyncPending = eventArgs.AcceptSocket!.ReceiveAsync(eventArgs);
        if (!isAsyncPending)
        {
            ProcessReceive(eventArgs, isAsyncPending);
        }
    }

    private void OnIoOperationCompleted(object? sender, SocketAsyncEventArgs e)
    {
        if (e.LastOperation == SocketAsyncOperation.Disconnect)
        {
            ProcessDisconnect(e);
        }
        else if (e.LastOperation == SocketAsyncOperation.Receive)
        {
            ProcessReceive(e, isAsync: true);
        }
        else if (e.LastOperation == SocketAsyncOperation.Send)
        {
            ProcessSend(e);
        }
    }

    private void ProcessSend(SocketAsyncEventArgs socketAsyncEventArgs)
    {
        throw new NotImplementedException();
    }

    private void ProcessDisconnect(SocketAsyncEventArgs e)
    {
        logger.LogInformation("Disconnecting from {RemoteEndPoint}.", e.RemoteEndPoint);
        e.AcceptSocket!.Disconnect(reuseSocket: true);
        e.AcceptSocket!.Close();
        e.Completed -= OnIoOperationCompleted;
        _socketPool.Release(e);
        _bufferPool.Release(e);
    }

    public async ValueTask DisposeAsync()
    {
        logger.LogInformation("Cleaning up server...");
        if (_listenSocket.Connected)
        {
            await _listenSocket.DisconnectAsync(reuseSocket: false);
        }
        await _socketPool.DisposeAsync();
    }
}
