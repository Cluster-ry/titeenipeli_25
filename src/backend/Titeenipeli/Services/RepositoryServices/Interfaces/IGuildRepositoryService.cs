using Titeenipeli.Enums;
using Titeenipeli.Schema;

namespace Titeenipeli.Services.RepositoryServices.Interfaces;

public interface IGuildRepositoryService : IEntityRepositoryService<Guild>
{
    public Guild? GetByColor(int color);
    public Guild? GetByName(string name);
    public Guild? GetByNameId(GuildName nameId);
}