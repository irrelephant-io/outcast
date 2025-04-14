using System.Net;
using Irrelephant.Outcast.Protocol.DataTransfer.Encoding;
using Irrelephant.Outcast.Server.Configuration;
using Irrelephant.Outcast.Server.Hosting;
using Irrelephant.Outcast.Server.Networking;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using var factory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug).AddConsole());
var hostLogger = factory.CreateLogger<Program>();

var localhost = await Dns.GetHostEntryAsync(IPAddress.IPv6Loopback);
var options = Options.Create(new NetworkingOptions
{
    ConnectionListenEndpoint = new IPEndPoint(localhost.AddressList[0], port: 42069),
    MessageCodec = new JsonMessageCodec(),
    Logger = factory.CreateLogger("Networking")
});

await using var host = new ServerHost(TimeSpan.FromSeconds(5), hostLogger);
await using var server = new NetworkService(options);

hostLogger.LogInformation("Starting server...");

await server.RunAsync(host.CancellationToken);
