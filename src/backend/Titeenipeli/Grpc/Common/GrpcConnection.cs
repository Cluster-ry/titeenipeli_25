using Grpc.Core;
using Titeenipeli.Schema;

namespace Titeenipeli.Grpc.Common;

public class GrpcConnection<TResponseStream>(User user, IServerStreamWriter<TResponseStream> responseStream)
{
    private static int _idCounter = 0;
    public readonly int Id = Interlocked.Increment(ref _idCounter);
    public readonly User User = user;
    public readonly IServerStreamWriter<TResponseStream> ResponseStream = responseStream;
}
