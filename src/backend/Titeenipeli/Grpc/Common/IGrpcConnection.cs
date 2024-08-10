using System.Threading.Channels;
using Titeenipeli.Schema;

namespace Titeenipeli.Grpc.Common;

public interface IGrpcConnection<TResponseStream>
{
    public int Id { get; init; }
    public User User { get; set; }
    public Channel<TResponseStream> ResponseStreamQueue { get; init; }
}
