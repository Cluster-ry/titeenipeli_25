using System.Collections.Concurrent;
using GrpcGeneratedServices;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Enums;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Grpc.Common;
using Titeenipeli.Grpc.Services;

namespace Titeenipeli.Services.Grpc;

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
                MiscStateUpdateResponse incrementalResponse = new();

                if (gameStateUpdateInput.MaximumPixelBucket != 0)
                {
                    incrementalResponse.PixelBucket = new()
                    {
                        Amount = (uint)gameStateUpdateInput.User.PixelBucket,
                        MaxAmount = (uint)gameStateUpdateInput.MaximumPixelBucket,
                        IncreasePerMinute = gameStateUpdateInput.User.Guild.CurrentRateLimitIncreasePerMinutePerPlayer
                    };
                }

                if (gameStateUpdateInput.Guilds != null)
                {
                    var scores = GuildsToScores(gameStateUpdateInput.Guilds);
                    incrementalResponse.Scores.Add(scores);
                }

                if (gameStateUpdateInput.PowerUps != null)
                {
                    var grpcPowerUps = ConvertPowerupsToGrpc(gameStateUpdateInput.PowerUps);
                    incrementalResponse.PowerUps.Add(grpcPowerUps);
                }

                await responseStream.WriteAsync(incrementalResponse);
            }
        }
        catch (Exception exception)
        {
            logger.LogError("Error while processing gRPC state updates: {error}", exception);
        }
    }

    private static List<MiscStateUpdateResponse.Types.Scores> GuildsToScores(List<Guild> guilds)
    {
        List<MiscStateUpdateResponse.Types.Scores> scores = [];
        foreach (var guild in guilds)
        {
            MiscStateUpdateResponse.Types.Scores score = new()
            {
                Guild = ConvertGuildToPixelGuild(guild.Name),
                Amount = (uint)guild.CurrentScore
            };
            scores.Add(score);
        }
        return scores;
    }

    private static PixelGuild ConvertGuildToPixelGuild(GuildName? guildName)
    {
        bool success = Enum.TryParse(guildName.ToString(), false, out PixelGuild result);
        return success ? result : PixelGuild.Nobody;
    }

    private static List<MiscStateUpdateResponse.Types.PowerUps> ConvertPowerupsToGrpc(List<PowerUp> powerUps)
    {
        List<MiscStateUpdateResponse.Types.PowerUps> grpcPowerUps = [];
        foreach (var powerUp in powerUps)
        {
            MiscStateUpdateResponse.Types.PowerUps grpcPowerUp = new()
            {
                PowerUpId = (uint)powerUp.PowerId,
                Name = powerUp.Name,
                Description = powerUp.Description
            };
            grpcPowerUps.Add(grpcPowerUp);
        }
        return grpcPowerUps;
    }
}