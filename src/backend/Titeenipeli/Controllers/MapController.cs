using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Enums;
using Titeenipeli.Inputs;
using Titeenipeli.Models;
using Titeenipeli.Options;
using Titeenipeli.Results;
using Titeenipeli.Results.CustomStatusCodes;
using Titeenipeli.Schema;
using Titeenipeli.Services;
using Titeenipeli.Services.RepositoryServices.Interfaces;

namespace Titeenipeli.Controllers;

[ApiController]
[Route("map/pixels")]
[Authorize(Policy = "MustHaveGuild")]
public class MapController : ControllerBase
{
    private const int BorderWidth = 1;
    private readonly GameOptions _gameOptions;

    private readonly IGameEventRepositoryService _gameEventRepositoryService;
    private readonly IMapRepositoryService _mapRepositoryService;
    private readonly IUserRepositoryService _userRepositoryService;

    private readonly RateLimitService _rateLimitService;

    public MapController(GameOptions gameOptions,
                         RateLimitService rateLimitService,
                         IUserRepositoryService userRepositoryService,
                         IMapRepositoryService mapRepositoryService,
                         IGameEventRepositoryService gameEventRepositoryService)
    {
        _gameOptions = gameOptions;
        _rateLimitService = rateLimitService;
        _userRepositoryService = userRepositoryService;
        _mapRepositoryService = mapRepositoryService;
        _gameEventRepositoryService = gameEventRepositoryService;
    }

    [HttpGet]
    public IActionResult GetPixels()
    {
        // TODO: Remove temporary testing user
        User? user = _userRepositoryService.GetByCode("test");

        if (user == null)
        {
            return BadRequest();
        }

        User[] users = _userRepositoryService.GetAll().ToArray();
        Pixel[] pixels = _mapRepositoryService.GetAll().ToArray();

        // +2 to account for the borders
        int width = _gameOptions.Width + 2 * BorderWidth;
        int height = _gameOptions.Height + 2 * BorderWidth;

        Map map = ConstructMap(pixels, width, height, user);
        MarkSpawns(map, users);
        map = CalculateFogOfWar(map);

        GetPixelsResult result = new GetPixelsResult
        {
            PlayerSpawn = new Coordinate
            {
                X = user.SpawnX,
                Y = user.SpawnY
            },
            Pixels = map.Pixels
        };

        return Ok(result);
    }

    [HttpPost]
    public IActionResult PostPixels([FromBody] PostPixelsInput pixelsInput)
    {
        // TODO: Remove temporary testing user
        User? user = _userRepositoryService.GetByCode("test");

        if (user == null)
        {
            return BadRequest();
        }

        if (!_rateLimitService.CanPlacePixel(user))
        {
            return new TooManyRequestsResult("Try again later", TimeSpan.FromSeconds(2147483646));
        }


        Coordinate globalCoordinate = new Coordinate
        {
            X = user.SpawnX + pixelsInput.X,
            Y = user.SpawnY + pixelsInput.Y
        };

        if (IsValidPlacement(globalCoordinate, user))
        {
            return BadRequest();
        }

        Pixel? pixelToUpdate = _mapRepositoryService.GetByCoordinate(globalCoordinate);

        if (pixelToUpdate == null)
        {
            return BadRequest();
        }

        if (pixelToUpdate.User != null &&
            pixelToUpdate.User.SpawnX == globalCoordinate.X &&
            pixelToUpdate.User.SpawnY == globalCoordinate.Y)
        {
            return BadRequest();
        }


        pixelToUpdate.User = user;
        _mapRepositoryService.Update(pixelToUpdate);

        user.LastPlacement = DateTime.UtcNow;
        _userRepositoryService.Update(user);

        GameEvent gameEvent = new GameEvent
        {
            User = user,
            // TODO: This is only temporary, fix this when GameEvent structure is more clear
            Event = JsonSerializer.Serialize("{ " +
                                             "   'eventType': 'SetPixel'," +
                                             "   'coordinates': {" +
                                             "       'x': " + globalCoordinate.X + "," +
                                             "       'y': " + globalCoordinate.Y + "," +
                                             "   }" +
                                             "}")
        };

        _gameEventRepositoryService.Add(gameEvent);

        return Ok();
    }

    private static Map ConstructMap(IEnumerable<Pixel> pixels, int width, int height, User? user)
    {
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
                Type = PixelType.Normal,
                Owner = (GuildName?)pixel.User?.Guild?.Color,
                // TODO: Verify owning status of pixel, this can be done when we get user information from JWT
                OwnPixel = pixel.User == user
            };

            map.Pixels[pixel.X + 1, pixel.Y + 1] = mapPixel;
        }

        return map;
    }

    private static void MarkSpawns(Map map, IEnumerable<User> users)
    {
        foreach (User user in users) map.Pixels[user.SpawnX, user.SpawnY].Type = PixelType.Spawn;
    }

    private Map CalculateFogOfWar(Map map)
    {
        int width = map.Pixels.GetLength(0);
        int height = map.Pixels.GetLength(1);
        Map fogOfWarMap = new Map
        {
            Pixels = new PixelModel[width, height],
            Width = width,
            Height = height
        };

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map.Pixels[x, y].OwnPixel)
                {
                    fogOfWarMap = MarkPixelsInFogOfWar(fogOfWarMap, map, new Coordinate
                    {
                        X = x,
                        Y = y
                    }, _gameOptions.FogOfWarDistance);
                }
            }
        }

        return TrimMap(fogOfWarMap);
    }

    private static Map MarkPixelsInFogOfWar(Map fogOfWarMap, Map map, Coordinate pixel,
                                            int fogOfWarDistance)
    {
        int minX = int.Clamp(pixel.X - fogOfWarDistance, 0, map.Width - 1);
        int minY = int.Clamp(pixel.Y - fogOfWarDistance, 0, map.Height - 1);
        int maxX = int.Clamp(pixel.X + fogOfWarDistance, 0, map.Width - 1);
        int maxY = int.Clamp(pixel.Y + fogOfWarDistance, 0, map.Height - 1);

        fogOfWarMap.MinViewableX = int.Min(minX - 1, fogOfWarMap.MinViewableX);
        fogOfWarMap.MinViewableY = int.Min(minY - 1, fogOfWarMap.MinViewableY);
        fogOfWarMap.MaxViewableX = int.Max(maxX + 1, fogOfWarMap.MaxViewableX);
        fogOfWarMap.MaxViewableY = int.Max(maxY + 1, fogOfWarMap.MaxViewableY);

        for (int x = minY; x <= maxY; x++)
        {
            for (int y = minX; y <= maxX; y++)
            {
                fogOfWarMap.Pixels[x, y] = map.Pixels[x, y];
            }
        }

        return fogOfWarMap;
    }

    private static Map TrimMap(Map map)
    {
        Map trimmedMap = new Map
        {
            Pixels = new PixelModel[map.MaxViewableX - (map.MinViewableX + 1),
                map.MaxViewableY - (map.MinViewableY + 1)],
            Width = map.MaxViewableX - (map.MinViewableX + 1),
            Height = map.MaxViewableY - (map.MinViewableY + 1)
        };

        int offsetX = map.MinViewableX + 1;
        int offsetY = map.MinViewableY + 1;

        for (int x = 0; x < map.Width; x++)
        {
            if (map.MinViewableX >= x || x >= map.MaxViewableX)
            {
                continue;
            }

            for (int y = 0; y < map.Height; y++)
            {
                if (map.MinViewableY >= y || y >= map.MaxViewableY)
                {
                    continue;
                }

                trimmedMap.Pixels[x - offsetX, y - offsetY] = map.Pixels[x, y];
            }
        }

        return trimmedMap;
    }

    private bool IsValidPlacement(Coordinate pixelCoordinate, User user)
    {
        // Take neighboring pixels for the pixel the user is trying to set,
        // but remove cornering pixels and only return pixels belonging to
        // the user
        return (from pixel in _mapRepositoryService.GetAll()
                where Math.Abs(pixel.X - pixelCoordinate.X) <= 1 &&
                      Math.Abs(pixel.Y - pixelCoordinate.Y) <= 1 &&
                      Math.Abs(pixel.X - pixelCoordinate.X) + Math.Abs(pixel.Y - pixelCoordinate.Y) <= 1 &&
                      pixel.User == user
                select pixel).Any();
    }
}