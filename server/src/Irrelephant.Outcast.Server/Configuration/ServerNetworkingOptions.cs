using System.Net;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Encoding;
using Microsoft.Extensions.Logging;

namespace Irrelephant.Outcast.Server.Configuration;

public class ServerNetworkingOptions
{
    public int MaxConnectionBacklog { get; set; } = 8;

    public required IPEndPoint ConnectionListenEndpoint { get; set; }

    public required ILogger Logger { get; set; }

    public required IMessageCodec MessageCodec { get; set; }
}
