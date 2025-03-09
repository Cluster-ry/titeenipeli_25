using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;

namespace Titeenipeli.InMemoryProvider.MapProvider;

public class MapDatabaseWriterService : IDisposable
{
    public Channel<Pixel> PixelChannel { get; } = Channel.CreateUnbounded<Pixel>();

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Task _writePixelsTask;

    public MapDatabaseWriterService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _writePixelsTask = Task.Run(Write);
    }

    private async Task Write()
    {
        await foreach (var pixel in PixelChannel.Reader.ReadAllAsync())
        {
            var scope = _scopeFactory.CreateScope();
            var mapRepositoryService = scope.ServiceProvider.GetRequiredService<IMapRepositoryService>();
            var userRepositoryService = scope.ServiceProvider.GetRequiredService<IUserRepositoryService>();

            if (pixel.User is not null)
            {
                pixel.User = userRepositoryService.GetById(pixel.User.Id);
            }

            mapRepositoryService.Update(pixel);
            await mapRepositoryService.SaveChangesAsync();
        }
    }

    public void Dispose()
    {
        PixelChannel.Writer.Complete();
        _writePixelsTask.Wait();
        _writePixelsTask.Dispose();
        GC.SuppressFinalize(this);
    }
}