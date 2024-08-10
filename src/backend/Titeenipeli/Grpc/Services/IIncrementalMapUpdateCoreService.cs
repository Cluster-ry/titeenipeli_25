using GrpcGeneratedServices;
using Titeenipeli.Grpc.Common;

namespace Titeenipeli.Grpc.Services;

public interface IIncrementalMapUpdateCoreService
{
    public void AddGrpcConnection(IGrpcConnection<IncrementalMapUpdateResponse> connection);

    public void RemoveGrpcConnection(IGrpcConnection<IncrementalMapUpdateResponse> connection);
}
