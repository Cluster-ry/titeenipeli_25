using Titeenipeli.Context;
using Titeenipeli.Schema;
using Titeenipeli.Services.Interfaces;

namespace Titeenipeli.Services;

public class GuildService : IGuildService
{
    private readonly ApiDbContext _dbContext;

    public GuildService(ApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Guild? GetGuild(int id)
    {
        return _dbContext.Guilds.FirstOrDefault(guild => guild.Id == id);
    }

    public Guild? GetGuildByColor(int color)
    {
        return _dbContext.Guilds.FirstOrDefault(guild => guild.Color == color);
    }
}