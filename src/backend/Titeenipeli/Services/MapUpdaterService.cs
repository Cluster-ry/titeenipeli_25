using System.Text.Json;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;
using Titeenipeli.GameLogic;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Helpers;
using Titeenipeli.InMemoryProvider.MapProvider;
using Titeenipeli.InMemoryProvider.UserProvider;
using Titeenipeli.Options;
using Titeenipeli.Services.BackgroundServices;
using Titeenipeli.Services.Grpc;

namespace Titeenipeli.Services;

public class MapUpdaterService(
    IServiceScopeFactory scopeFactory,
    GameOptions gameOptions,
    IIncrementalMapUpdateCoreService incrementalMapUpdateCoreService,
    IMapProvider mapProvider,
    IUserProvider userProvider,
    ChannelProcessorBackgroundService channelProcessorBackgroundService
) : IMapUpdaterService
{
    private const int BorderWidth = 1;

    private readonly MapUpdater _mapUpdater = new();

    public Task<bool> PlacePixel(Coordinate pixelCoordinate, User newOwner)
    {
        var borderfiedCoordinate = pixelCoordinate + new Coordinate(1, 1);

        return channelProcessorBackgroundService.Enqueue(() =>
        {
            if (!PixelPlacement.IsValidPlacement(mapProvider, pixelCoordinate, newOwner) ||
                mapProvider.IsSpawn(pixelCoordinate))
            {
                return false;
            }

            var map = GetMap();
            var changedPixels = _mapUpdater.PlacePixel(map, borderfiedCoordinate, newOwner);

            DoGrpcUpdate(map, changedPixels);
            DoDatabaseUpdate(changedPixels, newOwner);


            return true;
        });
    }

    public Task<bool> PlacePixels(List<Coordinate> pixelCoordinates, User newOwner)
    {
        return channelProcessorBackgroundService.Enqueue(() =>
        {
            var grpcBatch = PlacePixelsWithRetry(pixelCoordinates, newOwner);
            DoGrpcUpdate(GetMap(), grpcBatch);


            return true;
        });
    }

    public Task<User> PlaceSpawn(User user)
    {
        return channelProcessorBackgroundService.Enqueue(() =>
        {
            var spawnGeneratorService = scopeFactory.CreateScope()
                                                    .ServiceProvider
                                                    .GetRequiredService<SpawnGeneratorService>();


            var map = GetMap();
            var spawnPoint = spawnGeneratorService.GetSpawnPoint(user.Guild.Name);

            user.SpawnX = spawnPoint.X;
            user.SpawnY = spawnPoint.Y;

            var changedPixels = _mapUpdater.PlacePixel(map, spawnPoint + new Coordinate(1, 1), user, PixelType.Spawn);

            DoGrpcUpdate(map, changedPixels);
            DoDatabaseUpdate(changedPixels, user);

            return user;
        });
    }

    private List<MapChange> PlacePixelsWithRetry(List<Coordinate> pixelCoordinates, User newOwner)
    {
        var map = GetMap();
        // I hope C# would not reallocate an object every time inside the select LINQ method even if we were to create
        // this inside the select, but I am too lazy to profile this and not taking any risks
        var addOneCoordinate = new Coordinate(1, 1);
        var changedPixels = _mapUpdater.PlacePixels(map,
            pixelCoordinates.Select(coordinate => coordinate + addOneCoordinate).ToArray(),
            newOwner);

        DoDatabaseUpdate(changedPixels, newOwner);

        return changedPixels;
    }

    private PixelWithType[,] GetMap()
    {
        var users = userProvider.GetAll().ToArray();
        var pixels = mapProvider.GetAll().ToArray();

        int width = gameOptions.Width + 2 * BorderWidth;
        int height = gameOptions.Height + 2 * BorderWidth;

        var map = new PixelWithType[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    map[x, y] = new PixelWithType
                    {
                        Type = PixelType.MapBorder
                    };
                }
            }
        }

        foreach (var pixel in pixels)
        {
            PixelWithType mapPixel = new()
            {
                Location = new Coordinate(pixel.X, pixel.Y),
                Type = PixelType.Normal,
                Owner = pixel.User
            };

            map[pixel.X + 1, pixel.Y + 1] = mapPixel;
        }

        MarkSpawns(map, users);

        return map;
    }

    private static void MarkSpawns(PixelWithType[,] map, IEnumerable<User> users)
    {
        foreach (var user in users.Where(user => !user.IsGod))
            map[user.SpawnX + 1, user.SpawnY + 1].Type = PixelType.Spawn;
    }

    private void DoGrpcUpdate(PixelWithType[,] pixels, List<MapChange> changedPixels)
    {
        Dictionary<Coordinate, GrpcChangePixel> nearbyPixels = [];
        List<MapChange> changes = [];
        foreach (var changedPixel in changedPixels)
        {
            LoopNearbyPixelsInsideFogOfWar(coordinate =>
            {
                var pixel = pixels[coordinate.X + 1, coordinate.Y + 1];
                nearbyPixels[coordinate] = new GrpcChangePixel
                {
                    Coordinate = coordinate,
                    User = pixel.Owner
                };
            }, changedPixel.Coordinate);

            changes.Add(changedPixel);
        }

        GrpcMapChangesInput mapChanges = new(nearbyPixels, changes);
        incrementalMapUpdateCoreService.UpdateUsersMapState(mapChanges);
    }

    private void DoDatabaseUpdate(List<MapChange> changedPixels, User newOwner)
    {
        List<object> gameEvents = [];
        foreach (var changedPixel in changedPixels)
        {
            var computedNewOwner = changedPixel.NewOwner == null ? null : newOwner;

            gameEvents.Add(new
            {
                fromUser = new
                {
                    userId = changedPixel.NewOwner?.Id,
                    userName = changedPixel.NewOwner?.Username,
                    guildId = changedPixel.NewOwner?.Guild.Id,
                    guildName = changedPixel.NewOwner?.Guild.Name
                },
                toUser = new
                {
                    userId = computedNewOwner?.Id,
                    userName = computedNewOwner?.Username,
                    guildId = computedNewOwner?.Guild.Id,
                    guildName = computedNewOwner?.Guild.Name
                },
                pixel = new
                {
                    x = changedPixel.Coordinate.X,
                    y = changedPixel.Coordinate.Y
                }
            });

            Pixel newPixel = new()
            {
                X = changedPixel.Coordinate.X,
                Y = changedPixel.Coordinate.Y,
                User = changedPixel.NewOwner
            };

            mapProvider.Update(newPixel);
        }

        var gameEvent = new GameEvent
        {
            Event = JsonSerializer.Serialize(gameEvents)
        };

        using (var scope = scopeFactory.CreateScope())
        {
            var gameEventRepositoryService = scope.ServiceProvider.GetRequiredService<IGameEventRepositoryService>();
            gameEventRepositoryService.Add(gameEvent);
            gameEventRepositoryService.SaveChanges();
        }
    }

    private void LoopNearbyPixelsInsideFogOfWar(Action<Coordinate> action, Coordinate aroundCoordinate)
    {
        int fogOfWarDistance = gameOptions.MaxFogOfWarDistance * 2;
        int minY = Math.Clamp(aroundCoordinate.Y - fogOfWarDistance, 0, gameOptions.Height);
        int maxY = Math.Clamp(aroundCoordinate.Y + fogOfWarDistance, 0, gameOptions.Height);
        int minX = Math.Clamp(aroundCoordinate.X - fogOfWarDistance, 0, gameOptions.Width);
        int maxX = Math.Clamp(aroundCoordinate.X + fogOfWarDistance, 0, gameOptions.Width);

        Coordinate coordinate = new();
        for (coordinate.Y = minY; coordinate.Y <= maxY; coordinate.Y++)
        {
            for (coordinate.X = minX; coordinate.X <= maxX; coordinate.X++)
            {
                action(coordinate);
            }
        }
    }
}