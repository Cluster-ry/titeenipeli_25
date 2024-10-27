using Titeenipeli.Enumeration;
using Titeenipeli.Schema;

namespace Titeenipeli.Services.RepositoryServices.Interfaces;

public interface IGuildRepositoryService : IEntityRepositoryService<Guild>
{
    public Guild? GetByName(GuildName name);
    public void Update(Guild guild);
}