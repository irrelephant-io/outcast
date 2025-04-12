using System.Net.Sockets;

namespace Irrelephant.Outcast.Server.Networking.EventModel;

public class OutboundMessageAvailableArgs : EventArgs
{
    public required SocketAsyncEventArgs Socket { get; init; }
    public required Memory<byte> MessageContents { get; init; }
}
