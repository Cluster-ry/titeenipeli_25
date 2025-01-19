using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;
using Titeenipeli.Common.Results;
using Titeenipeli.Common.Results.CustomStatusCodes;
using Titeenipeli.Extensions;
using Titeenipeli.InMemoryMapProvider;
using Titeenipeli.Inputs;
using Titeenipeli.Options;
using Titeenipeli.Services;

namespace Titeenipeli.Controllers;

[ApiController]
[Route("state/map/pixels")]
[Authorize]
public class MapController : ControllerBase
{
    private const int BorderWidth = 1;

    private readonly GameOptions _gameOptions;

    private readonly IUserRepositoryService _userRepositoryService;
    private readonly IBackgroundGraphicsService _backgroundGraphicsService;

    private readonly IMapUpdaterService _mapUpdaterService;
    private readonly IMapProvider _mapProvider;

    private readonly IJwtService _jwtService;

    public MapController(GameOptions gameOptions,
                         IJwtService jwtService,
                         IUserRepositoryService userRepositoryService,
                         IMapProvider mapProvider,
                         IMapUpdaterService mapUpdaterService,
                         IBackgroundGraphicsService backgroundGraphicsService)
    {
        _gameOptions = gameOptions;
        _userRepositoryService = userRepositoryService;
        _mapProvider = mapProvider;
        _jwtService = jwtService;
        _mapUpdaterService = mapUpdaterService;
        _backgroundGraphicsService = backgroundGraphicsService;
    }

    [HttpGet]
    public IActionResult GetPixels()
    {
        var user = HttpContext.GetUser(_jwtService, _userRepositoryService);

        var users = _userRepositoryService.GetAll().ToArray();
        var pixels = _mapProvider.GetAll().ToArray();

        // +2 to account for the borders
        int width = _gameOptions.Width + 2 * BorderWidth;
        int height = _gameOptions.Height + 2 * BorderWidth;

        var map = ConstructMap(pixels, width, height, user);
        MarkSpawns(map, users);
        map = CalculateFogOfWar(map, user.Id);
        InjectBackgroundGraphics(map);
        var inversedMap = InverseMap(map);

        var result = new GetPixelsResult
        {
            PlayerSpawn = new Coordinate
            {
                X = user.SpawnX - map.MinViewableX,
                Y = user.SpawnY - map.MinViewableY
            },
            Pixels = inversedMap.Pixels
        };

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> PostPixels([FromBody] PostPixelsInput pixelsInput)
    {
        var user = HttpContext.GetUser(_jwtService, _userRepositoryService);

        if (user.PixelBucket < 1)
        {
            return new TooManyRequestsResult("Try again later", TimeSpan.FromMinutes(1));
        }

        var globalCoordinate = new Coordinate
        {
            X = user.SpawnX + pixelsInput.X,
            Y = user.SpawnY + pixelsInput.Y
        };

        if (!await _mapUpdaterService.PlacePixel(_userRepositoryService, globalCoordinate, user))
        {
            string description = new Random().NextInt64(100) == 69
                ? "Have a token #I_DONT_KNOW_THE_RULES"
                : "Try another pixel";

            var error = new ErrorResult
            {
                Title = "Invalid pixel placement",
                Code = ErrorCode.InvalidPixelPlacement,
                Description = description
            };

            return BadRequest(error);
        }

        user.PixelBucket--;
        _userRepositoryService.Update(user);
        await _userRepositoryService.SaveChangesAsync();

        return Ok();
    }

    private static Map ConstructMap(IEnumerable<Pixel> pixels, int width, int height, User? user)
    {
        var map = new Map
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
                        Type = PixelType.MapBorder
                    };
                }
            }
        }

        foreach (var pixel in pixels)
        {
            var mapPixel = new PixelModel
            {
                Type = PixelType.Normal,
                Guild = pixel.User?.Guild.Name,
                Owner = pixel.User?.Id ?? 0,
            };

            map.Pixels[pixel.X + 1, pixel.Y + 1] = mapPixel;
        }

        return map;
    }

    private static void MarkSpawns(Map map, IEnumerable<User> users)
    {
        foreach (var user in users) map.Pixels[user.SpawnX + 1, user.SpawnY + 1].Type = PixelType.Spawn;
    }

    private Map CalculateFogOfWar(Map map, int userId)
    {
        int width = map.Pixels.GetLength(0);
        int height = map.Pixels.GetLength(1);
        var fogOfWarMap = CreateEmptyMap(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map.Pixels[x, y].Owner == userId)
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

    private Map CreateEmptyMap(int width, int height)
    {
        var map = new Map
        {
            Pixels = new PixelModel[width, height],
            Width = width,
            Height = height
        };

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map.Pixels[x, y] = new PixelModel()
                {
                    Type = PixelType.FogOfWar
                };
            }
        }

        return map;
    }

    private static Map MarkPixelsInFogOfWar(Map fogOfWarMap,
                                            Map map,
                                            Coordinate pixel,
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

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                fogOfWarMap.Pixels[x, y] = map.Pixels[x, y];
            }
        }

        return fogOfWarMap;
    }

    private static Map TrimMap(Map map)
    {
        var trimmedMap = new Map
        {
            Pixels = new PixelModel[map.MaxViewableX - (map.MinViewableX + 1),
                map.MaxViewableY - (map.MinViewableY + 1)],
            Width = map.MaxViewableX - (map.MinViewableX + 1),
            Height = map.MaxViewableY - (map.MinViewableY + 1),
            MinViewableX = map.MinViewableX,
            MinViewableY = map.MinViewableY,
            MaxViewableX = map.MaxViewableX,
            MaxViewableY = map.MaxViewableY
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

    private void InjectBackgroundGraphics(Map map)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map.Pixels[x, y].Type == PixelType.MapBorder)
                {
                    continue;
                }

                byte[]? backgroundGraphic =
                    _backgroundGraphicsService.GetBackgroundGraphic(new Coordinate(map.MinViewableX + x,
                        map.MinViewableY + y));
                if (backgroundGraphic == null)
                {
                    continue;
                }

                string backgroundGraphicWire = Convert.ToBase64String(backgroundGraphic);
                map.Pixels[x, y].BackgroundGraphic = backgroundGraphicWire;
            }
        }
    }

    private static Map InverseMap(Map map)
    {
        var inversedMap = new Map
        {
            Pixels = new PixelModel[map.Height, map.Width],
            Width = map.Height,
            Height = map.Width
        };
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                inversedMap.Pixels[y, x] = map.Pixels[x, y];
            }
        }
        return inversedMap;
    }
}