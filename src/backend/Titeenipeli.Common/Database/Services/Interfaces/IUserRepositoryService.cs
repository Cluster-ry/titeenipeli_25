using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Enums;

namespace Titeenipeli.Common.Database.Services.Interfaces;

public interface IUserRepositoryService : IEntityRepositoryService<User>
{
    public User? GetByCode(string code);
    public User? GetByTelegramId(string telegramId);
    public List<User> GetByGuild(GuildName guildName);
    public User? GetByAuthenticationToken(string token);
    public void Update(User user);
}