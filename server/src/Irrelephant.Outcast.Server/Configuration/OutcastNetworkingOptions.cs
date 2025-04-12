using System.Net;
using Irrelephant.Outcast.Protocol.Encoding;

namespace Irrelephant.Outcast.Server.Configuration;

public class OutcastNetworkingOptions
{
    public int MaxSimultaneousConnectionRequests { get; set; } = 4;

    public int MaxConnectionBacklog { get; set; } = 8;

    public required IPEndPoint ConnectionListenEndpoint { get; set; }

    public required IMessageCodec MessageCodec { get; set; }

    public int BufferSize { get; set; } = 256;

    public int MaxResizableBufferSize { get; set; } = 1024;
}
