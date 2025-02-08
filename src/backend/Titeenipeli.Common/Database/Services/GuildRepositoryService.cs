using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;

namespace Titeenipeli.Common.Database.Services;

public class GuildRepositoryService(ApiDbContext dbContext)
    : EntityRepositoryService(dbContext), IGuildRepositoryService
{
    public Guild? GetById(int id)
    {
        return DbContext.Guilds.FirstOrDefault(guild => guild.Id == id);
    }

    public List<Guild> GetAll()
    {
        return DbContext.Guilds.ToList();
    }

    public void Add(Guild guild)
    {
        DbContext.Guilds.Add(guild);
    }

    public Guild? GetByName(GuildName name)
    {
        return DbContext.Guilds.FirstOrDefault(guild => guild.Name == name);
    }

    public void Update(Guild guild)
    {
        var existingGuild = GetById(guild.Id);

        if (existingGuild == null)
        {
            throw new Exception("Guild doesn't exist.");
        }

        DbContext.Entry(existingGuild).CurrentValues.SetValues(guild);
    }
}