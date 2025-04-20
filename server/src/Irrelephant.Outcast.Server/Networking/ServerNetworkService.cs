using System.Net.Sockets;
using Irrelephant.Outcast.Networking.Transport;
using Irrelephant.Outcast.Server.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Irrelephant.Outcast.Server.Networking;

public class ServerNetworkService(
    IOptions<ServerNetworkingOptions> options,
    IOptions<ServerStorageOptions> storageOptions
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
        TcpTransportHandler
    }

    private void OnAcceptCompleted(object? sender, SocketAsyncEventArgs e)
    {

    }
}
