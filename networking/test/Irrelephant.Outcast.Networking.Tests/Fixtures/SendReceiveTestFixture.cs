using System.Net;
using System.Net.Sockets;

namespace Irrelephant.Outcast.Networking.Tests.Fixtures;

public class SendReceiveTestFixture : IAsyncLifetime
{
    public SocketAsyncEventArgs Server { get; private set; } = null!;
    public SocketAsyncEventArgs Client { get; private set; } = null!;

    private static async ValueTask<Socket> InitializeSocketAsync()
    {
        var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp)
        {
            Blocking = false
        };

        var localhost = await Dns.GetHostEntryAsync(IPAddress.IPv6Loopback);
        socket.Bind(new IPEndPoint(localhost.AddressList[0], 0));

        return socket;
    }

    public async Task InitializeAsync()
    {
        var taskCompletionSource = new TaskCompletionSource();
        var server = await InitializeSocketAsync();
        server.Listen();

        var serverAccepted = false;
        var clientConnected = false;

        var serverSocketOptions = new SocketAsyncEventArgs();
        serverSocketOptions.Completed += (_, args) =>
        {
            Server = args;

            if (clientConnected)
            {
                taskCompletionSource.SetResult();
            }
            else
            {
                serverAccepted = true;
            }
        };
        server.AcceptAsync(serverSocketOptions);
        var clientSocketOptions = new SocketAsyncEventArgs
        {
            RemoteEndPoint = server.LocalEndPoint
        };

        clientSocketOptions.Completed += (_, args) =>
        {
            Client = args;

            if (serverAccepted)
            {
                taskCompletionSource.SetResult();
            }
            else
            {
                clientConnected = true;
            }
        };


        Socket.ConnectAsync(SocketType.Stream, ProtocolType.Tcp, clientSocketOptions);


        await taskCompletionSource.Task;
    }

    public Task DisposeAsync()
    {
        Server.Dispose();
        Client.Dispose();

        return Task.CompletedTask;
    }
}
