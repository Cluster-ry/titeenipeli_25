using System.Threading.Channels;

namespace Titeenipeli.Services.BackgroundServices;

public class ChannelProcessorBackgroundService : BackgroundService
{
    private readonly ILogger<ChannelProcessorBackgroundService> _logger;
    private readonly Channel<Func<Task>> _channel;

    public ChannelProcessorBackgroundService(ILogger<ChannelProcessorBackgroundService> logger)
    {
        _logger = logger;
        _channel = Channel.CreateUnbounded<Func<Task>>();
    }

    public Task<T> Enqueue<T>(Func<T> task)
    {
        var tcs = new TaskCompletionSource<T>();

        _channel.Writer.TryWrite(() =>
        {
            try
            {
                var result = task();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in {ServiceName}", GetType().Name);
                tcs.SetException(ex);
            }

            return Task.CompletedTask;
        });

        return tcs.Task;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var task = await _channel.Reader.ReadAsync(stoppingToken);
                await task();
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in {ServiceName}", GetType().Name);
            }
        }
    }
}