using System.Net;
using System.Net.Sockets;
using Irrelephant.Outcast.Protocol.Encoding;
using Irrelephant.Outcast.Protocol.Messages;

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
    Console.WriteLine("<Enter> to start bind and start connecting from ::1:50000 ...");
    Console.ReadLine();

    var entry = await Dns.GetHostEntryAsync(IPAddress.IPv6Loopback);
    var address = entry.AddressList[0];

    socket.Bind(new IPEndPoint(address, 50000));

    Console.WriteLine("Bound! <Enter> to initiate connection ...");
    Console.ReadLine();

    await socket.ConnectAsync(new IPEndPoint(address, 42069));

    Console.WriteLine("Connected! <Enter> to send connect request message ...");
    Console.ReadLine();
    var codec = new JsonMessageCodec();

    var buffer = new byte[1024];
    var tlvMessage = codec.Encode(new ConnectRequest("john.doe@example.com"));
    var packedMessage = tlvMessage.PackInto(buffer);
    await socket.SendAsync(packedMessage, SocketFlags.None);

    Console.WriteLine("Sent! <Enter> to send more ...");
    Console.ReadLine();

    await socket.SendAsync(packedMessage, SocketFlags.None);
    await socket.SendAsync(packedMessage, SocketFlags.None);

    Console.WriteLine("Done! <Enter> to quit ...");
    Console.ReadLine();
}
finally
{
    if (socket.Connected)
    {
        await socket.DisconnectAsync(false);
    }

    socket.Dispose();
}
