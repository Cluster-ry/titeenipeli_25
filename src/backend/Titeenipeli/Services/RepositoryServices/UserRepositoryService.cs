using Microsoft.EntityFrameworkCore;
using Titeenipeli.Context;
using Titeenipeli.Schema;
using Titeenipeli.Services.RepositoryServices.Interfaces;

namespace Titeenipeli.Services.RepositoryServices;

public class UserRepositoryService : IUserRepositoryService
{
    private readonly ApiDbContext _dbContext;

    public UserRepositoryService(ApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

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