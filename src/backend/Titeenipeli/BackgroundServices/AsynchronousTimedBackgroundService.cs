namespace Titeenipeli.BackgroundServices;

public abstract class AsynchronousTimedBackgroundService<T>(ILogger<T> logger) : BackgroundService
{
    private string _serviceName => this.GetType().Name;

    protected readonly ILogger<T> Logger = logger;
    protected abstract TimeSpan Period { get; }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Periodic {ServiceName} is running!", _serviceName);

        try
        {
            await RunPeriodic(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Periodic {ServiceName} is stopping!", _serviceName);
        }
    }

    private async Task RunPeriodic(CancellationToken cancellationToken)
    {
        using PeriodicTimer timer = new(Period);
        do
        {
            await DoWorkWithErrorHandling();
        } while (await timer.WaitForNextTickAsync(cancellationToken));
    }

    private async Task DoWorkWithErrorHandling()
    {
        try
        {
            await DoWork();
        }
        catch (Exception exception)
        {
            if (exception is OperationCanceledException)
            {
                throw;
            }

            // Periodic services should not crash. Just log error, if service throws.
            Logger.LogError("Periodic {ServiceName} threw exception: {Exception}",
                _serviceName, exception);
        }
    }

    protected abstract Task DoWork();
}