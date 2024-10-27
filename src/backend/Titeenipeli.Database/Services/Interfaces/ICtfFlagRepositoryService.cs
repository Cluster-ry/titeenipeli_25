using Titeenipeli.Schema;

namespace Titeenipeli.Database.Services.Interfaces;

public interface ICtfFlagRepositoryService : IEntityRepositoryService<CtfFlag>
{
    public CtfFlag? GetByToken(string token);
}