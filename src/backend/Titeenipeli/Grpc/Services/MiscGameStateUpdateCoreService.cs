using System.Collections.Concurrent;
using GrpcGeneratedServices;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Grpc.Common;

namespace Titeenipeli.Grpc.Services;

public class MiscGameStateUpdateCoreService(ILogger<StateUpdateService> logger) :
    GrpcService<MiscStateUpdateResponse>, IMiscGameStateUpdateCoreService
{
    public async void UpdateMiscGameState(GrpcMiscGameStateUpdateInput gameStateUpdateInput)
    {
        var success = Connections.TryGetValue(gameStateUpdateInput.User.Id, out var userConnections);
        if (!success || userConnections == null)
        {
            return;
        }

        await SendGameUpdate(gameStateUpdateInput, userConnections);
    }

    private async Task SendGameUpdate(
        GrpcMiscGameStateUpdateInput gameStateUpdateInput,
        ConcurrentDictionary<int, IGrpcConnection<MiscStateUpdateResponse>> userConnections
        )
    {
        try
        {
            foreach (KeyValuePair<int, IGrpcConnection<MiscStateUpdateResponse>> userConnection in userConnections)
            {
                var responseStream = userConnection.Value.ResponseStreamQueue.Writer;
                MiscStateUpdateResponse incrementalResponse = new()
                {
                    PixelBucket = (uint)gameStateUpdateInput.User.PixelBucket
                };
                await responseStream.WriteAsync(incrementalResponse);
            }
        }
        catch (Exception exception)
        {
            logger.LogError("Error while processing gRPC state updates: {error}", exception);
        }
    }
}