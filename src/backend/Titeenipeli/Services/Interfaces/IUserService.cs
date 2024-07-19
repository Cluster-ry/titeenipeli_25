using Titeenipeli.Schema;

namespace Titeenipeli.Services.Interfaces;

public interface IUserService : IEntityService<User>
{
    public User? GetByCode(string code);
    public User? GetByTelegramId(string telegramId);
    public void Update(User user);
}