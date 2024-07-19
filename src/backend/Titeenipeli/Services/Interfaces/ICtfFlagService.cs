using Titeenipeli.Schema;

namespace Titeenipeli.Services.Interfaces;

public interface ICtfFlagService : IEntityService<CtfFlag>
{
    public CtfFlag? GetByToken(string token);
}