using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Enums;
using Titeenipeli.InMemoryProvider.UserProvider;

namespace Titeenipeli.IntegrationTest;

public class UserProviderStub : IUserProvider
{
    private List<User> _users = [];

    public void Initialize(List<User> users)
    {
        _users = users;
    }

    public User? GetById(int id)
    {
        lock (_users)
        {
            return _users.Find(user => user.Id == id);
        }
    }

    public List<User> GetAll()
    {
        lock (_users)
        {
            return _users;
        }
    }

    public User? GetByCode(string code)
    {
        lock (_users)
        {
            return _users.Find(user => user.Code == code);
        }
    }

    public User? GetByTelegramId(string telegramId)
    {
        lock (_users)
        {
            return _users.Find(user => user.TelegramId == telegramId);
        }
    }

    public List<User> GetByGuild(GuildName guildName)
    {
        lock (_users)
        {
            return _users.Where(user => user.Guild.Name == guildName).ToList();
        }
    }

    public User? GetByAuthenticationToken(string token)
    {
        lock (_users)
        {
            return _users.Find(user => user.AuthenticationToken == token);
        }
    }

    /// <summary>
    ///     Adds given user to user provider. Doesn't add user to database.
    /// </summary>
    /// <exception cref="InvalidOperationException">Check that given user is from database.</exception>
    /// <param name="user"><c>User</c> that already exists in database</param>
    public void Add(User user)
    {
        if (user.Id == 0)
        {
            throw new InvalidOperationException("User id cannot be zero. Check that given user is from database.");
        }

        lock (_users)
        {
            _users.Add(user);
        }
    }

    public void Update(User user)
    {
        lock (_users)
        {
            int userToUpdate = _users.FindIndex(existingUser => existingUser.Id == user.Id);

            if (userToUpdate == -1)
            {
                throw new Exception("User doesn't exist.");
            }

            _users[userToUpdate] = user;
        }
    }
}