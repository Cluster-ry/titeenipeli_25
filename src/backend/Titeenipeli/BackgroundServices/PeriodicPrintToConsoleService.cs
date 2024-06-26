namespace Titeenipeli.BackgroundServices;

public class PeriodicPrintToConsoleService(ILogger<PeriodicPrintToConsoleService> logger) : BackgroundService
{
    private readonly TimeSpan period = TimeSpan.FromMinutes(1);
    private readonly ILogger<PeriodicPrintToConsoleService> _logger = logger;

    private int _executionCount;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Periodic print to console service is running!");

        DoWork();

        using PeriodicTimer timer = new(period);
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                DoWork();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Periodic print to console service is stopping!");
        }
    }

    private void DoWork()
    {
        _executionCount++;

        _logger.LogInformation("Periodic print to console: #{_executionCount}", _executionCount);
    }
}