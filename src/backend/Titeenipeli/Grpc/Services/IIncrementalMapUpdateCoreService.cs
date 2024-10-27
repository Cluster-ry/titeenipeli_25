using GrpcGeneratedServices;
using Titeenipeli.Grpc.ChangeEntities;

namespace Titeenipeli.Grpc.Services;

public interface IIncrementalMapUpdateCoreService : IGrpcService<IncrementalMapUpdateResponse>
{
    public void UpdateUsersMapState(GrpcMapChangesInput mapChangesInput);
}