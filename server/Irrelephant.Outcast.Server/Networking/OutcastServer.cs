using System.Net;
using System.Net.Sockets;
using Irrelephant.Outcast.Protocol;
using Irrelephant.Outcast.Protocol.Encoding;
using Irrelephant.Outcast.Protocol.Messages;

namespace Irrelephant.Outcast.Server.Networking;

public record ConnectedClient(string Name, EndPoint RemoteEndPoint);

public class OutcastServer : IDisposable
{
    private IMessageCodec _codec = new JsonMessageCodec();

    private List<ConnectedClient> _connectedClients = [];

    private readonly byte[] _readBuffer = new byte[1024];
    private TlvHeader _tlvHeader;
    private int _receivedSoFar;

    // First incoming message should be the TLV header
    private bool _isReadingHeader = true;

    private readonly Socket _sessionChannel = new(
        AddressFamily.InterNetworkV6,
        SocketType.Stream,
        ProtocolType.Tcp
    )
    {
        Blocking = false
    };

    public async ValueTask RunAsync(CancellationToken cancellationToken)
    {
        await SetUpConnection(cancellationToken);
    }

    private async Task SetUpConnection(CancellationToken cancellationToken)
    {
        var entry = await Dns.GetHostEntryAsync(IPAddress.IPv6Loopback);
        var ipAddress = entry.AddressList[0];
        _sessionChannel.Bind(new IPEndPoint(ipAddress, port: 42069));
        _sessionChannel.Listen(backlog: 8);

        while (!cancellationToken.IsCancellationRequested)
        {
            var connectingSocket = await _sessionChannel.AcceptAsync(cancellationToken);
            var received = await connectingSocket.ReceiveAsync(
                _readBuffer.AsMemory(
                    _isReadingHeader ? 0 : _receivedSoFar,
                    _isReadingHeader
                        ? TlvHeader.Size
                        : _tlvHeader.MessageLength - _receivedSoFar
                ),
                cancellationToken
            );

            if (_isReadingHeader)
            {
                if (received < TlvHeader.Size)
                {
                    throw new NotImplementedException("Can't read fragmented TLV headers.");
                }

                _tlvHeader = TlvHeader.Unpack(_readBuffer);
                _isReadingHeader = false;
            }
            else
            {
                if (received + _receivedSoFar < _tlvHeader.MessageLength)
                {
                    _receivedSoFar += received;
                    continue;
                }

                var message = new TlvMessage(
                    _tlvHeader,
                    _readBuffer.AsMemory(start: 0, length: _tlvHeader.MessageLength)
                );
                var decodedMessage = _codec.Decode(message);
                if (decodedMessage is ConnectRequest cr)
                {
                    _connectedClients.Add(
                        new ConnectedClient(
                            cr.Name,
                            connectingSocket.RemoteEndPoint!
                        )
                    );
                }

                _isReadingHeader = true;
            }
        }
    }

    public void Dispose()
    {
        _sessionChannel.Dispose();
    }
}
