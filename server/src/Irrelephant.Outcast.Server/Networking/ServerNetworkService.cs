using System.Collections.Concurrent;
using System.Net.Sockets;
using Irrelephant.Outcast.Networking.Transport;
using Irrelephant.Outcast.Networking.Transport.Abstractions;
using Irrelephant.Outcast.Server.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Irrelephant.Outcast.Server.Networking;

public class ServerNetworkService(
    IOptions<ServerNetworkingOptions> options
) : IAsyncDisposable
{
    private readonly TaskCompletionSource _completionSource = new();

    public readonly List<ProtocolClient> ActiveClients = new();

    private Lock _clientAcceptLock = new();

    private readonly Socket _listenSocket = new(
        AddressFamily.InterNetworkV6,
        SocketType.Stream,
        ProtocolType.Tcp
    )
    {
        Blocking = false
    };

    public Task RunAsync(CancellationToken cancellationToken)
    {
        var configuration = options.Value;
        _listenSocket.Bind(configuration.ConnectionListenEndpoint);
        _listenSocket.Listen(configuration.MaxConnectionBacklog);

        options.Value.Logger.LogInformation(
            "Bound and listening on {EndPoint}.",
            configuration.ConnectionListenEndpoint
        );

        StartAccept(_listenSocket);

        cancellationToken.Register(() => _completionSource.TrySetCanceled(cancellationToken));
        return _completionSource.Task;
    }

    private void StartAccept(Socket listenSocket)
    {
        var acceptArgs = new SocketAsyncEventArgs();
        acceptArgs.Completed += OnAcceptCompleted;

        var isAsync = listenSocket.AcceptAsync(acceptArgs);
        if (!isAsync)
        {
            ProcessAccept(acceptArgs);
        }
    }

    private void ProcessAccept(SocketAsyncEventArgs acceptArgs)
    {
        var handler = TcpTransportHandler.FromSocketAsyncEventArgs(acceptArgs);
        var protocolQueue = new ProtocolClient(handler, options.Value.MessageCodec);
        lock (_clientAcceptLock)
        {
            ActiveClients.Add(protocolQueue);
        }
        StartAccept(_listenSocket);
    }

    private void OnAcceptCompleted(object? sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError == SocketError.Success)
        {
            ProcessAccept(e);
        }
    }

    public ValueTask DisposeAsync()
    {
        _listenSocket.Dispose();

        lock (_clientAcceptLock)
        {
            foreach (var client in ActiveClients)
            {
                client.Dispose();
            }
        }

        return ValueTask.CompletedTask;
    }
}
