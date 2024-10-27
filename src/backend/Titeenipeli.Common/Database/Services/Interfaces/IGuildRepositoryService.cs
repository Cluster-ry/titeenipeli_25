using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Enums;

namespace Titeenipeli.Common.Database.Services.Interfaces;

public interface IGuildRepositoryService : IEntityRepositoryService<Guild>
{
    public Guild? GetByName(GuildName name);
    public void Update(Guild guild);
}