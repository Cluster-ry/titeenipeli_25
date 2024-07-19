using Titeenipeli.Schema;

namespace Titeenipeli.Services.Interfaces;

public interface IUserService
{
    public User? GetUser(int id);
    public User? GetUserByCode(string code);
    public User? GetUserByTelegramId(string telegramId);
    public void AddUser(User user);
    public void UpdateUser(User user);
}