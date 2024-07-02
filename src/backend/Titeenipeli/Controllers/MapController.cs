using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Titeenipeli.Context;
using Titeenipeli.Enums;
using Titeenipeli.Models;
using Titeenipeli.Options;
using Titeenipeli.Schema;

namespace Titeenipeli.Controllers;

[ApiController]
[Route("map")]
public class MapController : ControllerBase
{
    private const int BorderWidth = 1;

    private readonly ApiDbContext _dbContext;
    private readonly GameOptions _gameOptions;

    public MapController(GameOptions gameOptions, ApiDbContext dbContext)
    {
        _gameOptions = gameOptions;
        _dbContext = dbContext;
    }

    [HttpGet("pixels")]
    public IActionResult GetPixels()
    {
        // TODO: Remove temporary testing user
        User? testUser = _dbContext.Users.FirstOrDefault(user => user.Code == "test");
        User[] users = _dbContext.Users.ToArray();
        Pixel[] pixels = _dbContext.Map
                                   .Include(pixel => pixel.User)
                                   .ThenInclude(user => user!.Guild)
                                   .OrderBy(pixel => pixel.Y).ToArray();

        // +2 to account for the borders
        int width = _gameOptions.Width + 2 * BorderWidth;
        int height = _gameOptions.Height + 2 * BorderWidth;

        MapModel map = ConstructMap(pixels, width, height, testUser);
        MarkSpawns(map, users);
        map = CalculateFogOfWar(map);

        return Ok(map);
    }

    [HttpPost("pixels")]
    public IActionResult PostPixels([FromBody] CoordinateModel pixelCoordinate)
    {
        // TODO: Remove temporary testing user
        User? testUser = _dbContext.Users.FirstOrDefault(user => user.Code == "test");

        if (testUser == null)
        {
            return BadRequest();
        }

        CoordinateModel globalCoordinate = new CoordinateModel
        {
            X = testUser.SpawnX + pixelCoordinate.X,
            Y = testUser.SpawnY + pixelCoordinate.Y
        };

        // Take neighboring pixels for the pixel the user is trying to set,
        // but remove cornering pixels and only return pixels belonging to
        // the user
        bool validPlacement = (from pixel in _dbContext.Map
                               where Math.Abs(pixel.X - globalCoordinate.X) <= 1 &&
                                     Math.Abs(pixel.Y - globalCoordinate.Y) <= 1 &&
                                     Math.Abs(pixel.X - globalCoordinate.X) + Math.Abs(pixel.Y - globalCoordinate.Y) <=
                                     1 &&
                                     pixel.User == testUser
                               select pixel).Any();

        if (!validPlacement)
        {
            return BadRequest();
        }

        Pixel? pixelToUpdate = (from pixel in _dbContext.Map
                                where pixel.X == globalCoordinate.X && pixel.Y == globalCoordinate.Y
                                select pixel).FirstOrDefault();

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


        pixelToUpdate.User = testUser;
        _dbContext.GameEvents.Add(new GameEvent
        {
            User = testUser,
            // TODO: This is only temporary, fix this when GameEvent structure is more clear
            Event = JsonSerializer.Serialize("{ " +
                                             "   'eventType': 'SetPixel'," +
                                             "   'coordinates': {" +
                                             "       'x': " + globalCoordinate.X + "," +
                                             "       'y': " + globalCoordinate.Y + "," +
                                             "   }" +
                                             "}")
        });

        _dbContext.SaveChanges();

        return Ok();
    }

    private static MapModel ConstructMap(IEnumerable<Pixel> pixels, int width, int height, User? user)
    {
        MapModel map = new MapModel
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
                        Type = PixelTypeEnum.MapBorder
                    };
                }
            }
        }

        foreach (Pixel pixel in pixels)
        {
            PixelModel mapPixel = new PixelModel
            {
                Type = PixelTypeEnum.Normal,
                Owner = (GuildEnum?)pixel.User?.Guild.Color,
                // TODO: Verify owning status of pixel, this can be done when we get user information from JWT
                OwnPixel = pixel.User == user
            };

            map.Pixels[pixel.X + 1, pixel.Y + 1] = mapPixel;
        }

        return map;
    }

    private static void MarkSpawns(MapModel map, IEnumerable<User> users)
    {
        foreach (User user in users) map.Pixels[user.SpawnX, user.SpawnY].Type = PixelTypeEnum.Spawn;
    }

    private MapModel CalculateFogOfWar(MapModel map)
    {
        int width = map.Pixels.GetLength(0);
        int height = map.Pixels.GetLength(1);
        MapModel fogOfWarMap = new MapModel
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
                    fogOfWarMap = MarkPixelsInFogOfWar(fogOfWarMap, map, new CoordinateModel
                    {
                        X = x,
                        Y = y
                    }, _gameOptions.FogOfWarDistance);
                }
            }
        }

        return TrimMap(fogOfWarMap);
    }

    private static MapModel MarkPixelsInFogOfWar(MapModel fogOfWarMap, MapModel map, CoordinateModel pixel,
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

    private static MapModel TrimMap(MapModel map)
    {
        MapModel trimmedMap = new MapModel
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
}