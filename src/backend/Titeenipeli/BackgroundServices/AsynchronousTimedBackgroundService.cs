namespace Titeenipeli.BackgroundServices;

public interface IAsynchronousTimedBackgroundService
{
    Task DoWork();
}

public class AsynchronousTimedBackgroundService<Service, ServiceImplementation>(
        IServiceProvider services,
        ILogger<ServiceImplementation> logger,
        TimeSpan period
    ) : BackgroundService where Service : IAsynchronousTimedBackgroundService
{
    private string _serviceName => this.GetType().Name;
    private readonly IServiceProvider _services = services;
    private readonly ILogger<ServiceImplementation> _logger = logger;
    private readonly TimeSpan _period = period;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Periodic {ServiceName} is running!", _serviceName);

        try
        {
            await RunPeriodic(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Periodic {ServiceName} is stopping!", _serviceName);
        }
    }

    private async Task RunPeriodic(CancellationToken cancellationToken)
    {
        using PeriodicTimer timer = new(_period);
        do
        {
            await DoWorkWithErrorHandling();
        } while (await timer.WaitForNextTickAsync(cancellationToken));
    }

    private async Task DoWorkWithErrorHandling()
    {
        try
        {
            using var scope = _services.CreateScope();
            var scopedBackgroundService = scope.ServiceProvider.GetRequiredService<Service>();

            await scopedBackgroundService.DoWork();
        }
        catch (Exception exception)
        {
            if (exception is OperationCanceledException)
            {
                throw;
            }

            // Periodic services should not crash. Just log error, if service throws.
            _logger.LogError("Periodic {ServiceName} threw exception: {Exception}",
                _serviceName, exception);
        }
    }
}