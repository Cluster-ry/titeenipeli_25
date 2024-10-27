using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Enums;
using Titeenipeli.Extensions;
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

    private readonly IMapRepositoryService _mapRepositoryService;
    private readonly IUserRepositoryService _userRepositoryService;
    private readonly IMapUpdaterService _mapUpdaterService;

    private readonly JwtService _jwtService;

    public MapController(GameOptions gameOptions,
                         JwtService jwtService,
                         IUserRepositoryService userRepositoryService,
                         IMapRepositoryService mapRepositoryService,
                         IMapUpdaterService mapUpdaterService)
    {
        _gameOptions = gameOptions;
        _userRepositoryService = userRepositoryService;
        _mapRepositoryService = mapRepositoryService;
        _jwtService = jwtService;
        _mapUpdaterService = mapUpdaterService;
    }

    [HttpGet]
    public IActionResult GetPixels()
    {
        JwtClaim? jwtClaim = HttpContext.GetUser(_jwtService);

        if (jwtClaim == null)
        {
            return BadRequest();
        }

        User? user = _userRepositoryService.GetById(jwtClaim.Id);

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
        map = CalculateFogOfWar(map, user.Id);
        Map inversedMap = InverseMap(map);

        GetPixelsResult result = new GetPixelsResult
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
        JwtClaim? jwtClaim = HttpContext.GetUser(_jwtService);

        if (jwtClaim == null)
        {
            return BadRequest();
        }

        User? user = _userRepositoryService.GetById(jwtClaim.Id);

        if (user == null)
        {
            return BadRequest();
        }

        if (user.PixelBucket < 1)
        {
            return new TooManyRequestsResult("Try again later", TimeSpan.FromMinutes(1));
        }

        Coordinate globalCoordinate = new Coordinate
        {
            X = user.SpawnX + pixelsInput.X,
            Y = user.SpawnY + pixelsInput.Y
        };

        if (!IsValidPlacement(globalCoordinate, user))
        {
            ErrorResult error = new ErrorResult
            {
                Title = "Invalid pixel placement",
                Code = ErrorCode.InvalidPixelPlacement,
                Description = "Try another pixel"
            };

            return BadRequest(error);
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
            ErrorResult error = new ErrorResult
            {
                Title = "Pixel is a spawn point",
                Code = ErrorCode.PixelIsSpawnPoint,
                Description = "Spawn pixels cannot be captured"
            };

            return BadRequest(error);
        }


        await _mapUpdaterService.PlacePixel(globalCoordinate, user);

        user.PixelBucket--;
        _userRepositoryService.Update(user);

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
                Guild = pixel.User?.Guild?.Name,
                Owner = pixel.User?.Id ?? 0,
            };

            map.Pixels[pixel.X + 1, pixel.Y + 1] = mapPixel;
        }

        return map;
    }

    private static void MarkSpawns(Map map, IEnumerable<User> users)
    {
        foreach (User user in users) map.Pixels[user.SpawnX + 1, user.SpawnY + 1].Type = PixelType.Spawn;
    }

    private Map CalculateFogOfWar(Map map, int userId)
    {
        int width = map.Pixels.GetLength(0);
        int height = map.Pixels.GetLength(1);
        Map fogOfWarMap = CreateEmptyMap(width, height);

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
                map.Pixels[x, y] = new PixelModel()
                {
                    Type = PixelType.FogOfWar
                };
            }
        }

        return map;
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
        Map trimmedMap = new Map
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

    private static Map InverseMap(Map map)
    {
        Map inversedMap = new Map
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