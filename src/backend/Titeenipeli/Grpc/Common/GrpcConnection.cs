using System.Threading.Channels;
using Grpc.Core;
using Titeenipeli.Schema;

namespace Titeenipeli.Grpc.Common;

public class GrpcConnection<TResponseStream> : IGrpcConnection<TResponseStream>
{
    private const int _maxChannelSize = 100;
    private static int _idCounter = 0;
    public int Id { get; init; } = Interlocked.Increment(ref _idCounter);
    public User User { get; set; }
    public Channel<TResponseStream> ResponseStreamQueue { get; init; } = Channel.CreateBounded<TResponseStream>(_maxChannelSize);
    private readonly IServerStreamWriter<TResponseStream> _responseStream;

    public GrpcConnection(User user, IServerStreamWriter<TResponseStream> responseStream)
    {
        User = user;
        _responseStream = responseStream;
        new Thread(ProcessResponseWrites).Start();
    }

    private async void ProcessResponseWrites()
    {
        await foreach (TResponseStream mapChangesInput in ResponseStreamQueue.Reader.ReadAllAsync())
        {
            await _responseStream.WriteAsync(mapChangesInput);
        }
    }
}
