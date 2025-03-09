using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;

namespace Titeenipeli.InMemoryProvider.UserProvider;

public class UserDatabaseWriterService : IDisposable
{
    public Channel<User> UserChannel { get; } = Channel.CreateUnbounded<User>();

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Task _writeUsersTask;

    public UserDatabaseWriterService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _writeUsersTask = Task.Run(Write);
    }

    private async Task Write()
    {
        await foreach (var user in UserChannel.Reader.ReadAllAsync())
        {
            var scope = _scopeFactory.CreateScope();
            var userRepositoryService = scope.ServiceProvider.GetRequiredService<IUserRepositoryService>();

            var existingUser = userRepositoryService.GetById(user.Id);

            if (existingUser is null)
            {
                continue;
            }

            existingUser.SpawnX = user.SpawnX;
            existingUser.SpawnY = user.SpawnY;
            existingUser.AuthenticationToken = user.AuthenticationToken;
            existingUser.AuthenticationTokenExpiryTime = user.AuthenticationTokenExpiryTime;
            existingUser.PixelBucket = user.PixelBucket;

            existingUser.PowerUps.Clear();
            existingUser.PowerUps.AddRange(user.PowerUps);

            userRepositoryService.Update(existingUser);

            await userRepositoryService.SaveChangesAsync();
        }
    }

    public void Dispose()
    {
        UserChannel.Writer.Complete();
        _writeUsersTask.Wait();
        _writeUsersTask.Dispose();
        GC.SuppressFinalize(this);
    }
}