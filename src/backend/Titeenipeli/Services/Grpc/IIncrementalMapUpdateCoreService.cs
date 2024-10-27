using GrpcGeneratedServices;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Grpc.Services;

namespace Titeenipeli.Services.Grpc;

public interface IIncrementalMapUpdateCoreService : IGrpcService<IncrementalMapUpdateResponse>
{
    public void UpdateUsersMapState(GrpcMapChangesInput mapChangesInput);
}