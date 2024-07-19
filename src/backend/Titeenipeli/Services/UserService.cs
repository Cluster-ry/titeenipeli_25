using Microsoft.EntityFrameworkCore;
using Titeenipeli.Context;
using Titeenipeli.Schema;
using Titeenipeli.Services.Interfaces;

namespace Titeenipeli.Services;

public class UserService : IUserService
{
    private readonly ApiDbContext _dbContext;

    public UserService(ApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public User? GetUser(int id)
    {
        return _dbContext.Users.Include(user => user.Guild).FirstOrDefault(user => user.Id == id);
    }

    public User? GetUserByCode(string code)
    {
        return _dbContext.Users.Include(user => user.Guild).FirstOrDefault(user => user.Code == code);
    }

    public User? GetUserByTelegramId(string telegramId)
    {
        return _dbContext.Users.Include(user => user.Guild).FirstOrDefault(user => user.TelegramId == telegramId);
    }

    public void AddUser(User user)
    {
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();
    }

    public void UpdateUser(User user)
    {
        User? existingUser = GetUser(user.Id);

        if (existingUser == null)
        {
            throw new Exception("User doesn't exist.");
        }

        _dbContext.Entry(existingUser).CurrentValues.SetValues(user);
        _dbContext.SaveChanges();
    }
}