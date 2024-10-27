using Titeenipeli.Common.Database.Schema;

namespace Titeenipeli.Common.Database.Services.Interfaces;

public interface ICtfFlagRepositoryService : IEntityRepositoryService<CtfFlag>
{
    public CtfFlag? GetByToken(string token);
}