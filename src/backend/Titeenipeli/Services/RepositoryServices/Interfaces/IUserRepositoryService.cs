using Titeenipeli.Enums;
using Titeenipeli.Schema;

namespace Titeenipeli.Services.RepositoryServices.Interfaces;

public interface IUserRepositoryService : IEntityRepositoryService<User>
{
    public User? GetByCode(string code);
    public User? GetByTelegramId(string telegramId);
    public User[] GetByGuild(GuildName guildName);
    public User? GetByAuthenticationToken(string token);
    public void Update(User user);
}