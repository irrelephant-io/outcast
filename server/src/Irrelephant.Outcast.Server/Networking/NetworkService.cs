using System.Net.Sockets;
using Irrelephant.Outcast.Server.Configuration;
using Irrelephant.Outcast.Server.Networking.EventModel;
using Irrelephant.Outcast.Server.Protocol.Client;
using Irrelephant.Outcast.Server.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Irrelephant.Outcast.Server.Networking;

public class NetworkService(
    IOptions<NetworkingOptions> options
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

    private readonly IoBufferPool _bufferPool = new(options);

    private readonly IoSocketPool _socketPool = new(options);

    public Task RunAsync(CancellationToken cancellationToken)
    {
        var configuration = options.Value;
        _listenSocket.Bind(configuration.ConnectionListenEndpoint);
        _listenSocket.Listen(configuration.MaxConnectionBacklog);

        options.Value.Logger.LogInformation(
            "Bound and listening on {EndPoint}.",
            configuration.ConnectionListenEndpoint
        );

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
        var ioSocket = _socketPool.BorrowForNewClient();
        var client = ioSocket.GetAssociatedClient();
        options.Value.Logger.LogInformation(
            "Incoming TCP connection from `{RemoteEndPoint}`. Establishing session `{SessionId}`",
            eventArgs.AcceptSocket!.RemoteEndPoint,
            client.SessionId
        );
        ioSocket.Completed += OnIoOperationCompleted;
        client.MessageHandler.OutboundMessageAvailable += OnOutboundAvailable;
        ioSocket.AcceptSocket = eventArgs.AcceptSocket;
        _bufferPool.Assign(ioSocket);

        var isAsyncPending = ioSocket.AcceptSocket!.ReceiveAsync(ioSocket);
        if (!isAsyncPending)
        {
            ProcessReceive(ioSocket, isAsyncPending);
        }
    }

    private void OnOutboundAvailable(object? sender, OutboundMessageAvailableArgs args)
    {
        options.Value.Logger.LogInformation(
            "Sending data to session `{SessionId}`.",
            args.Socket.GetAssociatedClient().SessionId
        );
        args.MessageContents.CopyTo(args.Socket.MemoryBuffer);
        args.Socket.SetBuffer(args.Socket.Offset, args.MessageContents.Length);
        args.Socket.AcceptSocket!.SendAsync(args.Socket);
    }

    private void ProcessReceive(SocketAsyncEventArgs eventArgs, bool isAsync)
    {
        if (eventArgs.BytesTransferred > 0 && eventArgs.SocketError == SocketError.Success)
        {
            options.Value.Logger.LogInformation(
                "Incoming data in session `{RemoteEndPoint}`.",
                eventArgs.GetAssociatedClient().SessionId
            );
            var client = (ServerSideConnectingClient)eventArgs.UserToken!;
            client.MessageHandler.ProcessRead(eventArgs.MemoryBuffer, eventArgs.BytesTransferred, isAsync);
            var isAsyncPending = eventArgs.AcceptSocket!.ReceiveAsync(eventArgs);
            if (!isAsyncPending)
            {
                ProcessReceive(eventArgs, isAsyncPending);
            }
        }
        else
        {
            ProcessDisconnect(eventArgs);
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
        }
    }

    private void ProcessDisconnect(SocketAsyncEventArgs e)
    {
        options.Value.Logger.LogInformation("Disconnecting from session `{SessionId}`.", e.GetAssociatedClient().SessionId);
        e.AcceptSocket!.Shutdown(SocketShutdown.Both);
        e.AcceptSocket!.Close();
        e.Completed -= OnIoOperationCompleted;
        e.GetAssociatedClient().MessageHandler.OutboundMessageAvailable -= OnOutboundAvailable;
        _socketPool.Release(e);
        _bufferPool.Release(e);
    }

    public async ValueTask DisposeAsync()
    {
        options.Value.Logger.LogInformation("Cleaning up server...");
        if (_listenSocket.Connected)
        {
            await _listenSocket.DisconnectAsync(reuseSocket: false);
        }
        await _socketPool.DisposeAsync();
    }
}
