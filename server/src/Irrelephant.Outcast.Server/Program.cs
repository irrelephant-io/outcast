using System.Net;
using Irrelephant.Outcast.Networking.Protocol;
using Irrelephant.Outcast.Server.Configuration;
using Irrelephant.Outcast.Server.Data.Storage;
using Irrelephant.Outcast.Server.Hosting;
using Irrelephant.Outcast.Server.Networking;
using Irrelephant.Outcast.Server.Simulation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using var factory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug).AddConsole());
var hostLogger = factory.CreateLogger<Program>();

var localhost = await Dns.GetHostEntryAsync(IPAddress.IPv6Loopback);
var options = Options.Create(new ServerNetworkingOptions
{
    ConnectionListenEndpoint = new IPEndPoint(localhost.AddressList[0], port: 42069),
    Logger = factory.CreateLogger("Networking"),
    MessageCodec = new JsonMessageCodec()
});

hostLogger.LogInformation("Initialising world...");
var world = new OutcastWorld(
    factory.CreateLogger<OutcastWorld>(),
    new StorageReader()
);

hostLogger.LogInformation("Starting server...");
await using var host = new ServerHost(TimeSpan.FromSeconds(5), hostLogger);
await using var server = new ServerNetworkService(options);

hostLogger.LogInformation("Starting simulation...");
var simulation = new ServerSimulator(world, server, factory.CreateLogger<ServerSimulator>());

await Task.WhenAll(
    server.RunAsync(host.CancellationToken),
    simulation.RunAsync(host.CancellationToken)
);
