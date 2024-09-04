using System.Text.Json;
using Titeenipeli.Enums;
using Titeenipeli.GameLogic;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Grpc.Services;
using Titeenipeli.Models;
using Titeenipeli.Options;
using Titeenipeli.Schema;
using Titeenipeli.Services.RepositoryServices.Interfaces;

namespace Titeenipeli.Services;

public class MapUpdaterWrapper(
        IServiceScopeFactory scopeFactory,
        GameOptions gameOptions,
        IIncrementalMapUpdateCoreService incrementalMapUpdateCoreService
    ) : IMapUpdaterWrapper
{
    private const int BorderWidth = 1;

    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly GameOptions _gameOptions = gameOptions;
    private readonly IIncrementalMapUpdateCoreService _incrementalMapUpdateCoreService = incrementalMapUpdateCoreService;

    private readonly MapUpdater _mapUpdater = new();

    public Task PlacePixel(Coordinate pixelCoordinates, User newOwner)
    {
        GuildName newGuild = (newOwner?.Guild?.Name) ?? throw new Exception("Unable to get guild name");
        var borderfiedCoordinate = pixelCoordinates + new Coordinate(1, 1);

        return Task.Run(() =>
        {
            lock (_mapUpdater)
            {
                (Map map, Pixel[,] pixelArray) = GetMap();
                List<(Coordinate, PixelModel)> changedPixels = _mapUpdater.PlacePixel(map, borderfiedCoordinate, newGuild);
                changedPixels.Add((borderfiedCoordinate, new PixelModel { Type = PixelType.Normal, Owner = newOwner.Guild.Name }));
                DoGrpcUpdate(pixelArray, changedPixels, newOwner);
                DoDatabaseUpdate(pixelArray, changedPixels, newOwner);
            }
        });
    }

    private (Map, Pixel[,]) GetMap()
    {
        Pixel[] pixels;
        using (var scope = _scopeFactory.CreateScope())
        {
            IMapRepositoryService mapRepositoryService = scope.ServiceProvider.GetRequiredService<IMapRepositoryService>();
            pixels = [.. mapRepositoryService.GetAll()];
        }
        int width = _gameOptions.Width + 2 * BorderWidth;
        int height = _gameOptions.Height + 2 * BorderWidth;

        Pixel[,] pixel2DArray = new Pixel[width, height];
        Map map = new Map
        {
            Pixels = new PixelModel[width, height],
            Width = width,
            Height = height
        };

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    map.Pixels[x, y] = new PixelModel
                    {
                        OwnPixel = false,
                        Type = PixelType.MapBorder
                    };
                }
            }
        }

        foreach (Pixel pixel in pixels)
        {
            PixelModel mapPixel = new PixelModel
            {
                Type = pixel.User?.SpawnX == pixel.X && pixel.User?.SpawnY == pixel.Y ? PixelType.Spawn : PixelType.Normal,
                Owner = pixel.User?.Guild?.Name,
                OwnPixel = false
            };

            pixel2DArray[pixel.X + 1, pixel.Y + 1] = pixel;
            map.Pixels[pixel.X + 1, pixel.Y + 1] = mapPixel;
        }

        return (map, pixel2DArray);
    }

    private void DoGrpcUpdate(Pixel[,] pixel2DArray, List<(Coordinate, PixelModel)> changedPixels, User newOwner)
    {
        Dictionary<Coordinate, GrpcChangePixel> nearbyPixels = [];
        List<GrpcMapChangeInput> changes = [];
        foreach ((Coordinate, PixelModel) changedPixel in changedPixels)
        {
            LoopNearbyPixelsInsideFogOfWar((coordinate) =>
            {
                Pixel pixel = pixel2DArray[coordinate.Y, coordinate.X];
                nearbyPixels[coordinate] = new()
                {
                    Coordinate = coordinate - new Coordinate(1, 1),
                    User = pixel?.User
                };
            }, changedPixel.Item1);

            changes.Add(new()
            {
                Coordinate = changedPixel.Item1 - new Coordinate(1, 1),
                OldOwner = pixel2DArray[changedPixel.Item1.X, changedPixel.Item1.Y].User,
                NewOwner = changedPixel.Item2.Owner == null ? null : newOwner
            });
        }

        GrpcMapChangesInput mapChanges = new(nearbyPixels, changes);
        _incrementalMapUpdateCoreService.UpdateUsersMapState(mapChanges);
    }

    private void DoDatabaseUpdate(Pixel[,] pixel2DArray, List<(Coordinate, PixelModel)> changedPixels, User newOwner)
    {
        List<object> gameEvents = new();
        foreach ((Coordinate, PixelModel) changedPixel in changedPixels)
        {
            Pixel pixel = pixel2DArray[changedPixel.Item1.X, changedPixel.Item1.Y];
            User? computedNewOwner = changedPixel.Item2.Owner == null ? null : newOwner;

            gameEvents.Add(new
            {
                fromUser = new
                {
                    userId = pixel.User?.Id,
                    userName = pixel.User?.Username,
                    guildId = pixel.User?.Guild?.Id,
                    guildName = pixel.User?.Guild?.Name
                },
                toUser = new
                {
                    userId = computedNewOwner?.Id,
                    userName = computedNewOwner?.Username,
                    guildId = computedNewOwner?.Guild?.Id,
                    guildName = computedNewOwner?.Guild?.Name
                },
                pixel = new
                {
                    x = changedPixel.Item1.X - 1,
                    y = changedPixel.Item1.Y - 1
                }
            });

            pixel.User = computedNewOwner;
            using (var scope = _scopeFactory.CreateScope())
            {
                IMapRepositoryService mapRepositoryService = scope.ServiceProvider.GetRequiredService<IMapRepositoryService>();
                mapRepositoryService.Update(pixel);
            }
        }

        GameEvent gameEvent = new GameEvent
        {
            Event = JsonSerializer.Serialize(gameEvents)
        };
        using (var scope = _scopeFactory.CreateScope())
        {
            IGameEventRepositoryService gameEventRepositoryService = scope.ServiceProvider.GetRequiredService<IGameEventRepositoryService>();
            gameEventRepositoryService.Add(gameEvent);
        }
    }

    private void LoopNearbyPixelsInsideFogOfWar(Action<Coordinate> action, Coordinate aroundCoordinate)
    {
        int fogOfWarDistance = _gameOptions.FogOfWarDistance * 2;
        int minY = Math.Clamp(aroundCoordinate.Y - fogOfWarDistance, 0, _gameOptions.Height);
        int maxY = Math.Clamp(aroundCoordinate.Y + fogOfWarDistance, 0, _gameOptions.Height);
        int minX = Math.Clamp(aroundCoordinate.X - fogOfWarDistance, 0, _gameOptions.Width);
        int maxX = Math.Clamp(aroundCoordinate.X + fogOfWarDistance, 0, _gameOptions.Width);

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