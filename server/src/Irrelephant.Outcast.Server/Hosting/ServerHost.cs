using Microsoft.Extensions.Logging;

namespace Irrelephant.Outcast.Server.Hosting;

public class ServerHost : IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();

    public CancellationToken CancellationToken => _cts.Token;

    public ServerHost(
        TimeSpan gracefulShutdownTimeout,
        ILogger hostLogger
    )
    {
        Console.CancelKeyPress += async (_, sigTermEventArgs) =>
        {
            sigTermEventArgs.Cancel = true;
            if (_cts.IsCancellationRequested)
            {
                return;
            }

            hostLogger.LogInformation("Stopping server host...");
            await _cts.CancelAsync();
        };

        AppDomain.CurrentDomain.ProcessExit += async (_, _) =>
        {
            if (_cts.IsCancellationRequested)
            {
                return;
            }

            hostLogger.LogInformation("Stopping server host...");
            await _cts.CancelAsync();
        };
    }

    public ValueTask DisposeAsync()
    {
        _cts.Dispose();
        return ValueTask.CompletedTask;
    }
}
