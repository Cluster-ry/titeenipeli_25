using Titeenipeli.Schema;

namespace Titeenipeli.Services.Interfaces;

public interface IGuildService
{
    public Guild? GetGuild(int id);
    public Guild? GetGuildByColor(int color);
}