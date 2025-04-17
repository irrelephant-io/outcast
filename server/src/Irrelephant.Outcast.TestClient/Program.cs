using System.Net;
using System.Net.Sockets;
using Irrelephant.Outcast.Protocol.Abstractions.DataTransfer;
using Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Encoding;
using Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Messages;

var socket = new Socket(
    AddressFamily.InterNetworkV6,
    SocketType.Stream,
    ProtocolType.Tcp
)
{
    Blocking = true
};

try
{
    Console.WriteLine("<Enter> to start bind and start connecting from ::1:any port ...");

    var entry = await Dns.GetHostEntryAsync(IPAddress.IPv6Loopback);
    var address = entry.AddressList[0];

    // port 0 binds to any available port
    socket.Bind(new IPEndPoint(address, port: 0));

    Console.WriteLine($"Bound to {socket.LocalEndPoint}! <Enter> to initiate connection ...");

    await socket.ConnectAsync(new IPEndPoint(address, 42069));

    Console.WriteLine("Connected! <Enter> to send connect request message ...");
    var codec = new JsonMessageCodec();

    var buffer = new byte[1024];
    var tlvMessage = codec.Encode(new ConnectRequest("john.doe@example.com"));
    var packedMessage = tlvMessage.PackInto(buffer);
    await socket.SendAsync(packedMessage, SocketFlags.None);

    Console.WriteLine("Sent! <Enter> to receive ...");

    var received = await socket.ReceiveAsync(buffer, SocketFlags.None);
    Console.WriteLine($"Received {received} bytes ...");
    var header = TlvHeader.Unpack(buffer);
    Console.WriteLine($"Unpacked header: {header} ...");
    var receivedTlv = new TlvMessage(header, buffer[TlvHeader.Size..received]);
    var receivedMessage = codec.Decode(receivedTlv);

    Console.WriteLine($"Received message: {receivedMessage} ...");
}
finally
{
    socket.Shutdown(SocketShutdown.Both);
    socket.Close();
}
