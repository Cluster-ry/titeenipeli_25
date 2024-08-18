using System.Threading.Channels;
using Grpc.Core;
using Titeenipeli.Schema;

namespace Titeenipeli.Grpc.Common;

public class GrpcConnection<TResponseStream> : IGrpcConnection<TResponseStream>
{
    private const int MaxChannelSize = 100;
    private static int _idCounter = 0;
    public int Id { get; init; } = Interlocked.Increment(ref _idCounter);
    public User User { get; set; }
    public Channel<TResponseStream> ResponseStreamQueue { get; init; } = Channel.CreateBounded<TResponseStream>(MaxChannelSize);
    public Task ProcessResponseWritesTask { get; init; }
    private readonly IServerStreamWriter<TResponseStream> _responseStream;

    public GrpcConnection(User user, IServerStreamWriter<TResponseStream> responseStream)
    {
        User = user;
        _responseStream = responseStream;
        ProcessResponseWritesTask = Task.Run(ProcessResponseWrites);
    }

    private async Task ProcessResponseWrites()
    {
        await foreach (TResponseStream mapChangesInput in ResponseStreamQueue.Reader.ReadAllAsync())
        {
            try
            {
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
        ResponseStreamQueue.Writer.TryComplete(null);
    }
}
