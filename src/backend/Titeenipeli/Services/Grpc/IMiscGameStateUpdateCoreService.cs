using GrpcGeneratedServices;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Grpc.Services;

namespace Titeenipeli.Services.Grpc;

public interface IMiscGameStateUpdateCoreService : IGrpcService<MiscStateUpdateResponse>
{
    public void UpdateMiscGameState(GrpcMiscGameStateUpdateInput gameStateUpdateInput);
}