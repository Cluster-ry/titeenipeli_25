using System.Text.Json;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;
using Titeenipeli.GameLogic;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Options;
using Titeenipeli.Services.Grpc;

namespace Titeenipeli.Services;

public class MapUpdaterService(
    IServiceScopeFactory scopeFactory,
    GameOptions gameOptions,
    IIncrementalMapUpdateCoreService incrementalMapUpdateCoreService
) : IMapUpdaterService
{
    private const int BorderWidth = 1;

    private readonly MapUpdater _mapUpdater = new();

    public Task PlacePixel(Coordinate pixelCoordinates, User newOwner)
    {
        var borderfiedCoordinate = pixelCoordinates + new Coordinate(1, 1);

        return Task.Run(() =>
        {
            lock (_mapUpdater)
            {
                PixelWithType[,] map = GetMap();
                List<MapChange> changedPixels =
                    _mapUpdater.PlacePixel(map, borderfiedCoordinate, newOwner);

                DoGrpcUpdate(map, changedPixels);
                DoDatabaseUpdate(changedPixels, newOwner);
            }
        });
    }

    private PixelWithType[,] GetMap()
    {
        User[] users;
        Pixel[] pixels;
        using (var scope = scopeFactory.CreateScope())
        {
            var userRepositoryService = scope.ServiceProvider
                                             .GetRequiredService<IUserRepositoryService>();

            users = userRepositoryService.GetAll().ToArray();
            var mapRepositoryService = scope.ServiceProvider
                                            .GetRequiredService<IMapRepositoryService>();

            pixels = mapRepositoryService.GetAll().ToArray();
        }

        int width = gameOptions.Width + 2 * BorderWidth;
        int height = gameOptions.Height + 2 * BorderWidth;

        PixelWithType[,] map = new PixelWithType[width, height];

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
        foreach (var user in users) map[user.SpawnX + 1, user.SpawnY + 1].Type = PixelType.Spawn;
    }

    private void DoGrpcUpdate(PixelWithType[,] pixels,
                              List<MapChange> changedPixels)
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
                    User = pixel?.Owner
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

            using (var scope = scopeFactory.CreateScope())
            {
                var mapRepositoryService = scope.ServiceProvider
                                                .GetRequiredService<IMapRepositoryService>();

                Pixel newPixel = new()
                {
                    X = changedPixel.Coordinate.X,
                    Y = changedPixel.Coordinate.Y,
                    User = changedPixel.NewOwner
                };

                mapRepositoryService.Update(newPixel);
            }
        }

        var gameEvent = new GameEvent
        {
            Event = JsonSerializer.Serialize(gameEvents)
        };

        using (var scope = scopeFactory.CreateScope())
        {
            var gameEventRepositoryService = scope.ServiceProvider
                                                  .GetRequiredService<IGameEventRepositoryService>();
            gameEventRepositoryService.Add(gameEvent);
        }
    }

    private void LoopNearbyPixelsInsideFogOfWar(Action<Coordinate> action,
                                                Coordinate aroundCoordinate)
    {
        int fogOfWarDistance = gameOptions.FogOfWarDistance * 2;
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