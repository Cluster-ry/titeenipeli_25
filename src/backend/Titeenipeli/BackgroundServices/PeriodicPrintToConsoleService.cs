namespace Titeenipeli.BackgroundServices;

public class PeriodicPrintToConsoleService(ILogger<PeriodicPrintToConsoleService> logger) :
    AsynchronousTimedBackgroundService<PeriodicPrintToConsoleService>(logger)
{
    protected override TimeSpan Period => TimeSpan.FromMinutes(1);

    private int _executionCount;

    protected override Task DoWork()
    {
        _executionCount++;

        Logger.LogInformation("Periodic print to console: #{_executionCount}", _executionCount);

        return Task.CompletedTask;
    }
}