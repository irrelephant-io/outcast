using System.Net;
using Irrelephant.Outcast.Protocol.Encoding;
using Irrelephant.Outcast.Server.Configuration;
using Irrelephant.Outcast.Server.Hosting;
using Irrelephant.Outcast.Server.Networking;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var localhost = await Dns.GetHostEntryAsync(IPAddress.IPv6Loopback);
var options = Options.Create(new OutcastNetworkingOptions
{
    ConnectionListenEndpoint = new IPEndPoint(localhost.AddressList[0], port: 42069),
    MessageCodec = new JsonMessageCodec()
});

using var factory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug).AddConsole());
var hostLogger = factory.CreateLogger<Program>();
await using var host = new ServerHost(TimeSpan.FromSeconds(5), hostLogger);
await using var server = new OutcastServer(options, factory.CreateLogger<OutcastServer>());

hostLogger.LogInformation("Starting server...");

await server.RunAsync(host.CancellationToken);
