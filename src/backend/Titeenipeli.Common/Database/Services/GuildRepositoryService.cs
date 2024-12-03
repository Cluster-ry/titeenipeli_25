using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;

namespace Titeenipeli.Common.Database.Services;

public class GuildRepositoryService(ApiDbContext dbContext) : RepositoryService(dbContext), IGuildRepositoryService
{
    public Guild? GetById(int id)
    {
        return _dbContext.Guilds.FirstOrDefault(guild => guild.Id == id);
    }

    public List<Guild> GetAll()
    {
        return _dbContext.Guilds.ToList();
    }

    public void Add(Guild guild)
    {
        _dbContext.Guilds.Add(guild);
        _dbContext.SaveChanges();
    }

    public Guild? GetByName(GuildName name)
    {
        return _dbContext.Guilds.FirstOrDefault(guild => guild.Name == name);
    }

    public void Update(Guild guild)
    {
        Guild? existingGuild = GetById(guild.Id);

        if (existingGuild == null)
        {
            throw new Exception("Guild doesn't exist.");
        }

        _dbContext.Entry(existingGuild).CurrentValues.SetValues(guild);
        _dbContext.SaveChanges();
    }
}