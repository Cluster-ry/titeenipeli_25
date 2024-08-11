using GrpcGeneratedServices;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Grpc.Common;

namespace Titeenipeli.Grpc.Services;

public interface IIncrementalMapUpdateCoreService
{
    public void UpdateUsersMapState(GrpcMapChangesInput mapChangesInput);
    public void AddGrpcConnection(IGrpcConnection<IncrementalMapUpdateResponse> connection);

    public void RemoveGrpcConnection(IGrpcConnection<IncrementalMapUpdateResponse> connection);
}
