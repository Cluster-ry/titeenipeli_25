using Microsoft.EntityFrameworkCore;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;

namespace Titeenipeli.Common.Database.Services;

public class UserRepositoryService(ApiDbContext dbContext) : EntityRepositoryService(dbContext), IUserRepositoryService
{
    public User? GetById(int id)
    {
        return DbContext.Users.Include(user => user.Guild)
                        .Include(user => user.PowerUps)
                        .FirstOrDefault(user => user.Id == id);
    }

    public List<User> GetAll()
    {
        return DbContext.Users.Include(user => user.Guild)
                        .Include(user => user.PowerUps)
                        .ToList();
    }

    public User? GetByCode(string code)
    {
        return DbContext.Users.Include(user => user.Guild)
                        .Include(user => user.PowerUps)
                        .FirstOrDefault(user => user.Code == code);
    }

    public User? GetByTelegramId(string telegramId)
    {
        return DbContext.Users.Include(user => user.Guild)
                        .Include(user => user.PowerUps)
                        .FirstOrDefault(user => user.TelegramId == telegramId);
    }

    public User[] GetByGuild(GuildName guildName)
    {
        return DbContext.Users.Include(user => user.Guild)
                        .Include(user => user.PowerUps)
                        .Where(user => user.Guild.Name == guildName).ToArray();
    }

    public User? GetByAuthenticationToken(string token)
    {
        return DbContext.Users.Include(user => user.Guild)
                        .Include(user => user.PowerUps)
                        .FirstOrDefault(user => user.AuthenticationToken == token);
    }

    public void Add(User user)
    {
        DbContext.Users.Add(user);
    }

    public void Update(User user)
    {
        var existingUser = GetById(user.Id);

        if (existingUser == null)
        {
            throw new Exception("User doesn't exist.");
        }

        DbContext.Entry(existingUser).CurrentValues.SetValues(user);
    }
}