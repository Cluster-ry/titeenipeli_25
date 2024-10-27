using GrpcGeneratedServices;
using Titeenipeli.Grpc.ChangeEntities;

namespace Titeenipeli.Grpc.Services;

public interface IMiscGameStateUpdateCoreService : IGrpcService<MiscStateUpdateResponse>
{
    public void UpdateMiscGameState(GrpcMiscGameStateUpdateInput gameStateUpdateInput);
}