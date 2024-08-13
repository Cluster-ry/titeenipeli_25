using Titeenipeli.Context;
using Titeenipeli.Enums;
using Titeenipeli.Schema;
using Titeenipeli.Services.RepositoryServices.Interfaces;

namespace Titeenipeli.Services.RepositoryServices;

public class GuildRepositoryService : IGuildRepositoryService
{
    private readonly ApiDbContext _dbContext;

    public GuildRepositoryService(ApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

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

    public Guild? GetByColor(int color)
    {
        return _dbContext.Guilds.FirstOrDefault(guild => guild.Color == color);
    }

    public Guild? GetByName(string name)
    {
        return _dbContext.Guilds.FirstOrDefault(guild => guild.Name == name);
    }

    public Guild? GetByNameId(GuildName nameId)
    {
        return _dbContext.Guilds.FirstOrDefault(guild => guild.NameId == nameId);
    }
}