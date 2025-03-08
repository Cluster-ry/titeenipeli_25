using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Enums;

namespace Titeenipeli.InMemoryProvider.UserProvider;

public interface IUserProvider
{
    public void Initialize(List<User> users);
    public User? GetById(int id);
    public List<User> GetAll();
    public User? GetByCode(string code);
    public User? GetByTelegramId(string telegramId);
    public List<User> GetByGuild(GuildName guildName);
    public User? GetByAuthenticationToken(string token);
    public void Add(User user);
    public void Update(User user);
}