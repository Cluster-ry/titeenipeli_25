using Titeenipeli.Grpc.Common;

namespace Titeenipeli.Grpc.Services;

public interface IGrpcService<TResponseStream> where TResponseStream : new()
{
    public void AddGrpcConnection(IGrpcConnection<TResponseStream> connection);

    public void RemoveGrpcConnection(IGrpcConnection<TResponseStream> connection);
}