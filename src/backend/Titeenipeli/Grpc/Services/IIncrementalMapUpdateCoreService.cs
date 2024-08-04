using GrpcGeneratedServices;
using Titeenipeli.Grpc.Common;

namespace Titeenipeli.Grpc.Services;

public interface IIncrementalMapUpdateCoreService
{
    public void AddGrpcConnection(GrpcConnection<IncrementalMapUpdateResponse> connection);

    public void RemoveGrpcConnection(GrpcConnection<IncrementalMapUpdateResponse> connection);
}
