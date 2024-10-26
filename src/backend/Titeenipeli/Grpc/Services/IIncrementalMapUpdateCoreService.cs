using Titeenipeli.Grpc.ChangeEntities;

namespace Titeenipeli.Grpc.Services;

public interface IIncrementalMapUpdateCoreService : IGrpcService
{
    public void UpdateUsersMapState(GrpcMapChangesInput mapChangesInput);
}