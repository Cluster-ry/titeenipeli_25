namespace Titeenipeli.BackgroundServices;

public interface IAsynchronousTimedBackgroundService
{
    Task DoWork();
}

public class AsynchronousTimedBackgroundService<TService, TServiceImplementation>(
    IServiceProvider services,
    ILogger<TServiceImplementation> logger,
    TimeSpan period
) : BackgroundService where TService : IAsynchronousTimedBackgroundService
{
    private string ServiceName => GetType().Name;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Periodic {ServiceName} is running!", ServiceName);

        try
        {
            await RunPeriodic(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Periodic {ServiceName} is stopping!", ServiceName);
        }
    }

    private async Task RunPeriodic(CancellationToken cancellationToken)
    {
        using PeriodicTimer timer = new PeriodicTimer(period);
        do
        {
            await DoWorkWithErrorHandling();
        } while (await timer.WaitForNextTickAsync(cancellationToken));
    }

    private async Task DoWorkWithErrorHandling()
    {
        try
        {
            using IServiceScope scope = services.CreateScope();
            TService scopedBackgroundService = scope.ServiceProvider.GetRequiredService<TService>();

            await scopedBackgroundService.DoWork();
        }
        catch (Exception exception)
        {
            if (exception is OperationCanceledException)
            {
                throw;
            }

            // Periodic services should not crash. Just log error, if service throws.
            logger.LogError("Periodic {ServiceName} threw exception: {Exception}",
                ServiceName, exception);
        }
    }
}