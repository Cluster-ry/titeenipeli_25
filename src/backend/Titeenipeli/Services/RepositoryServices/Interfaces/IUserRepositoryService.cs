using Titeenipeli.Schema;

namespace Titeenipeli.Services.RepositoryServices.Interfaces;

public interface IUserRepositoryService : IEntityRepositoryService<User>
{
    public User? GetByCode(string code);
    public User? GetByTelegramId(string telegramId);
    public void Update(User user);
}