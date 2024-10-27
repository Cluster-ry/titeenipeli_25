using Titeenipeli.Enumeration;
using Titeenipeli.Schema;

namespace Titeenipeli.Database.Services.Interfaces;

public interface IGuildRepositoryService : IEntityRepositoryService<Guild>
{
    public Guild? GetByName(GuildName name);
    public void Update(Guild guild);
}