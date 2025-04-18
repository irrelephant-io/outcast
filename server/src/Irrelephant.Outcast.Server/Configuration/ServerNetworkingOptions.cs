using System.Net;
using Irrelephant.Outcast.Protocol.Networking;
using Microsoft.Extensions.Logging;

namespace Irrelephant.Outcast.Server.Configuration;

public class ServerNetworkingOptions : NetworkingOptions
{
    public int MaxSimultaneousConnectionRequests { get; set; } = 4;

    public int MaxConnectionBacklog { get; set; } = 8;

    public required IPEndPoint ConnectionListenEndpoint { get; set; }

    public required ILogger Logger { get; set; }
}
