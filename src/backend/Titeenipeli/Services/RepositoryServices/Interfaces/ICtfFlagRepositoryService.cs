using Titeenipeli.Schema;

namespace Titeenipeli.Services.RepositoryServices.Interfaces;

public interface ICtfFlagRepositoryService : IEntityRepositoryService<CtfFlag>
{
    public CtfFlag? GetByToken(string token);
}