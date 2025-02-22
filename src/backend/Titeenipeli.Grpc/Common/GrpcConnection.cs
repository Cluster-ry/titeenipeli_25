using System.Threading.Channels;
using Grpc.Core;
using Titeenipeli.Common.Database.Schema;

namespace Titeenipeli.Grpc.Common;

public class GrpcConnection<TResponseStream> : IGrpcConnection<TResponseStream> where TResponseStream : new()
{
    private const int MaxChannelSize = 100;
    private const int KeepAliveFrequencyInSeconds = 15;
    private const int KeepAliveCheckFrequencyInSeconds = 5;
    private static int _idCounter = 0;
    public int Id { get; init; } = Interlocked.Increment(ref _idCounter);
    public User User { get; set; }
    public Channel<TResponseStream> ResponseStreamQueue { get; init; } = Channel.CreateBounded<TResponseStream>(MaxChannelSize);
    public Task ProcessResponseWritesTask { get; init; }
    private readonly IServerStreamWriter<TResponseStream> _responseStream;
    private readonly Action<IGrpcConnection<TResponseStream>> _unregisterMethod;
    private bool disposed = false;
    private DateTime _lastMessageSent = DateTime.Now;

    public GrpcConnection(User user, IServerStreamWriter<TResponseStream> responseStream, Action<IGrpcConnection<TResponseStream>> unregisterMethod)
    {
        User = user;
        _responseStream = responseStream;
        _unregisterMethod = unregisterMethod;
        ProcessResponseWritesTask = Task.Run(ProcessResponseWrites);
        Task.Run(KeepAlive);
    }

    private async Task KeepAlive()
    {
        Task queueCompletion = ResponseStreamQueue.Reader.Completion;
        while (!queueCompletion.IsCompleted)
        {
            await Task.Delay(KeepAliveCheckFrequencyInSeconds * 1000);

            if (_lastMessageSent > DateTime.Now - TimeSpan.FromSeconds(KeepAliveFrequencyInSeconds))
            {
                continue;
            }

            try
            {
                TResponseStream incrementalResponse = new();
                await ResponseStreamQueue.Writer.WriteAsync(incrementalResponse);
            }
            catch (Exception)
            {
                Dispose();
                break;
            }
        }
    }

    private async Task ProcessResponseWrites()
    {
        await foreach (TResponseStream mapChangesInput in ResponseStreamQueue.Reader.ReadAllAsync())
        {
            try
            {
                _lastMessageSent = DateTime.Now;
                await _responseStream.WriteAsync(mapChangesInput);
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }
        disposed = true;

        ResponseStreamQueue.Writer.TryComplete();
        _unregisterMethod(this);
    }
}
