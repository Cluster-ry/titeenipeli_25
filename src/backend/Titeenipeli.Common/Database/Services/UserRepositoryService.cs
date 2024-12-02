using Microsoft.EntityFrameworkCore;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;

namespace Titeenipeli.Common.Database.Services;

public class UserRepositoryService(ApiDbContext dbContext) : RepositoryService(dbContext), IUserRepositoryService
{
    public User? GetById(int id)
    {
        return _dbContext.Users.Include(user => user.Guild).FirstOrDefault(user => user.Id == id);
    }

    public List<User> GetAll()
    {
        return _dbContext.Users.ToList();
    }

    public User? GetByCode(string code)
    {
        return _dbContext.Users.Include(user => user.Guild).FirstOrDefault(user => user.Code == code);
    }

    public User? GetByTelegramId(string telegramId)
    {
        return _dbContext.Users.Include(user => user.Guild).FirstOrDefault(user => user.TelegramId == telegramId);
    }

    public User[] GetByGuild(GuildName guildName)
    {
        return _dbContext.Users.Include(user => user.Guild).Where(user => user.Guild != null && user.Guild.Name == guildName).ToArray();
    }

    public User? GetByAuthenticationToken(string token)
    {
        return _dbContext.Users.Include(user => user.Guild).FirstOrDefault(user => user.AuthenticationToken == token);
    }

    public void Add(User user)
    {
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();
    }

    public void Update(User user)
    {
        User? existingUser = GetById(user.Id);

        if (existingUser == null)
        {
            throw new Exception("User doesn't exist.");
        }

        _dbContext.Entry(existingUser).CurrentValues.SetValues(user);
        _dbContext.SaveChanges();
    }
}