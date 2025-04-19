using System.Net.Sockets;
using Irrelephant.Outcast.Protocol.Networking.Session;

namespace Irrelephant.Outcast.Server.Utility;

public static class SocketUtilities
{
    public static IClient GetAssociatedClient(this SocketAsyncEventArgs socket) =>
        (IClient)socket.UserToken!;
}
