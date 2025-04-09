using Irrelephant.Outcast.Server.Hosting;
using Irrelephant.Outcast.Server.Networking;
using Microsoft.Extensions.Logging;

using var factory = LoggerFactory.Create(builder => builder.AddConsole());
var hostLogger = factory.CreateLogger<Program>();
var host = new ServerHost(
    gracefulShutdownTimeout: TimeSpan.FromSeconds(5),
    hostLogger
);
using var server = new OutcastServer();

hostLogger.LogInformation("Starting server...");

await server.RunAsync(host.CancellationToken);
