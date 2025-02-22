using Titeenipeli.Common.Database.Schema;

namespace Titeenipeli.Grpc.ChangeEntities;

public struct GrpcMiscGameStateUpdateInput(User user)
{
    public User User = user;
    public int MaximumPixelBucket;
    public List<Guild>? Guilds = [];
    public List<PowerUp>? PowerUps = [];
    public string? Message;
}