using System.Threading.Channels;
using Titeenipeli.Schema;

namespace Titeenipeli.Grpc.Common;

public interface IGrpcConnection<TResponseStream> : IDisposable
{
    public int Id { get; init; }
    public User User { get; set; }
    public Channel<TResponseStream> ResponseStreamQueue { get; init; }
    public Task ProcessResponseWritesTask { get; init; }
}
