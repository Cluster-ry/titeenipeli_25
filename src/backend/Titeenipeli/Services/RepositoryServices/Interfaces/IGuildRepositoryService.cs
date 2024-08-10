using Titeenipeli.Enums;
using Titeenipeli.Schema;

namespace Titeenipeli.Services.RepositoryServices.Interfaces;

public interface IGuildRepositoryService : IEntityRepositoryService<Guild>
{
    public Guild? GetByColor(GuildName name);
}