using Titeenipeli.Common.Database.Schema;

namespace Titeenipeli.Grpc.ChangeEntities;

public struct GrpcMiscGameStateUpdateInput(User user)
{
    public User User = user;
}