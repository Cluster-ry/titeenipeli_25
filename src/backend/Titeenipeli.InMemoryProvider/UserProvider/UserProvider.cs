using Microsoft.Extensions.DependencyInjection;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;

namespace Titeenipeli.InMemoryProvider.UserProvider;

public class UserProvider : IUserProvider
{
    private List<User> _users = [];
    private readonly UserDatabaseWriterService _userDatabaseWriterService;

    private readonly IServiceScopeFactory _scopeFactory;

    public UserProvider(UserDatabaseWriterService userDatabaseWriterService, IServiceScopeFactory scopeFactory)
    {
        _userDatabaseWriterService = userDatabaseWriterService;
        _scopeFactory = scopeFactory;
    }

    public void Initialize(List<User> users)
    {
        _users = users;
    }

    public User? GetById(int id)
    {
        lock (_users)
        {
            var user = _users.FirstOrDefault(user => user.Id == id);

            return user == null ? null : UpdateUserGuild(user);
        }
    }

    public List<User> GetAll()
    {
        lock (_users)
        {
            var scope = _scopeFactory.CreateScope();
            var guildRepositoryService = scope.ServiceProvider.GetRequiredService<IGuildRepositoryService>();

            foreach (var user in _users)
                user.Guild = guildRepositoryService.GetById(user.Guild.Id) ??
                             throw new InvalidDataException("Invalid guild.");

            return _users;
        }
    }

    public User? GetByCode(string code)
    {
        lock (_users)
        {
            var user = _users.FirstOrDefault(user => user.Code == code);

            return user == null ? null : UpdateUserGuild(user);
        }
    }

    public User? GetByTelegramId(string telegramId)
    {
        lock (_users)
        {
            var user = _users.FirstOrDefault(user => user.TelegramId == telegramId);

            return user == null ? null : UpdateUserGuild(user);
        }
    }

    public List<User> GetByGuild(GuildName guildName)
    {
        lock (_users)
        {
            var users = _users.Where(user => user.Guild.Name == guildName).ToList();

            var scope = _scopeFactory.CreateScope();
            var guildRepositoryService = scope.ServiceProvider.GetRequiredService<IGuildRepositoryService>();

            foreach (var user in users)
                user.Guild = guildRepositoryService.GetById(user.Guild.Id) ??
                             throw new InvalidDataException("Invalid guild.");

            return users;
        }
    }

    public User? GetByAuthenticationToken(string token)
    {
        lock (_users)
        {
            var user = _users.FirstOrDefault(user => user.AuthenticationToken == token);

            return user == null ? null : UpdateUserGuild(user);
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

            _userDatabaseWriterService.UserChannel.Writer.TryWrite(user);
        }
    }

    private User UpdateUserGuild(User user)
    {
        var scope = _scopeFactory.CreateScope();
        var guildRepositoryService = scope.ServiceProvider.GetRequiredService<IGuildRepositoryService>();

        user.Guild = guildRepositoryService.GetById(user.Guild.Id) ??
                     throw new InvalidDataException("Invalid guild.");

        return user;
    }
}