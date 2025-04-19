using System.Net.Sockets;

namespace Irrelephant.Outcast.Networking.Transport.EventModel;

public class OutboundMessageAvailableArgs : EventArgs
{
    public required SocketAsyncEventArgs Socket { get; init; }
    public required Memory<byte> MessageContents { get; init; }
}
