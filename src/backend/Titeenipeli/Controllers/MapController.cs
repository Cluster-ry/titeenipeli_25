using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Titeenipeli.Context;
using Titeenipeli.Enums;
using Titeenipeli.Models;
using Titeenipeli.Schema;

namespace Titeenipeli.Controllers;

[ApiController]
[Route("map")]
public class MapController : ControllerBase
{
    private readonly ApiDbContext _dbContext;

    public MapController(ApiDbContext dbContext)
    {
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

        int width = _dbContext.Map.Max(pixel => pixel.X) + 3;
        int height = _dbContext.Map.Max(pixel => pixel.Y) + 3;

        MapModel map = ConstructMap(pixels, width, height, testUser);
        map = MarkSpawns(map, users);
        map = CalculateFogOfWar(map);

        return Ok(map);
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
        for (int y = 0; y < height; y++)
            if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
            {
                map.Pixels[x, y] = new PixelModel
                {
                    OwnPixel = false,
                    Type = PixelTypeEnum.MapBorder
                };
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

    private static MapModel MarkSpawns(MapModel map, IEnumerable<User> users)
    {
        foreach (User user in users) map.Pixels[user.SpawnX, user.SpawnY].Type = PixelTypeEnum.Spawn;
        return map;
    }

    private static MapModel CalculateFogOfWar(MapModel map)
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
        for (int y = 0; y < height; y++)
            if (map.Pixels[x, y].OwnPixel)
            {
                fogOfWarMap = MarkPixelsInFogOfWar(fogOfWarMap, map, y, y, 2);
            }

        return TrimMap(fogOfWarMap);
    }

    private static MapModel MarkPixelsInFogOfWar(MapModel fogOfWarMap, MapModel map, int pixelX, int pixelY,
        int fogOfWarDistance)
    {
        int minX = int.Clamp(pixelX - fogOfWarDistance, 0, map.Width - 1);
        int minY = int.Clamp(pixelY - fogOfWarDistance, 0, map.Height - 1);
        int maxX = int.Clamp(pixelX + fogOfWarDistance, 0, map.Width - 1);
        int maxY = int.Clamp(pixelY + fogOfWarDistance, 0, map.Height - 1);

        fogOfWarMap.MinViewableX = int.Min(minX - 1, fogOfWarMap.MinViewableX);
        fogOfWarMap.MinViewableY = int.Min(minY - 1, fogOfWarMap.MinViewableY);
        fogOfWarMap.MaxViewableX = int.Max(maxX + 1, fogOfWarMap.MaxViewableX);
        fogOfWarMap.MaxViewableY = int.Max(maxY + 1, fogOfWarMap.MaxViewableY);

        for (int x = minY; x <= maxY; x++)
        for (int y = minX; y <= maxX; y++)
            fogOfWarMap.Pixels[x, y] = map.Pixels[x, y];

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

        for (int x = 0, trimmedX = 0; x < map.Width; x++)
        {
            if (map.MinViewableX >= x || x >= map.MaxViewableX)
            {
                continue;
            }

            for (int y = 0, trimmedY = 0; y < map.Height; y++)
            {
                if (map.MinViewableY >= y || y >= map.MaxViewableY)
                {
                    continue;
                }

                trimmedMap.Pixels[trimmedX, trimmedY] = map.Pixels[x, y];
                trimmedY++;
            }

            trimmedX++;
        }

        return trimmedMap;
    }

    [HttpPost("pixels")]
    public IActionResult PostPixels([FromBody] Coordinate pixelCoordinate)
    {
        // TODO: Map relative coordinates to global coordinates
        // TODO: Remove temporary testing user
        User? testUser = _dbContext.Users.FirstOrDefault(user => user.Code == "test");
        Pixel? pixelToUpdate =
            _dbContext.Map.Include(pixel => pixel.User)
                .FirstOrDefault(pixel => pixel.X == pixelCoordinate.X && pixel.Y == pixelCoordinate.Y);

        if (pixelToUpdate == null)
        {
            return BadRequest();
        }

        if (pixelToUpdate.User != null &&
            pixelToUpdate.User.SpawnX == pixelCoordinate.X &&
            pixelToUpdate.User.SpawnY == pixelCoordinate.Y)
        {
            return BadRequest();
        }
        

        pixelToUpdate.User = testUser;
        if (testUser != null)
        {
            _dbContext.GameEvents.Add(new GameEvent
            {
                User = testUser,
                // TODO: This is only temporary, fix this when GameEvent structure is more clear
                Event = JsonSerializer.Serialize("{ " +
                                                 "   'eventType': 'SetPixel'," +
                                                 "   'coordinates': {" +
                                                 "       'x': " + pixelCoordinate.X + "," +
                                                 "       'y': " + pixelCoordinate.Y + "," +
                                                 "   }" +
                                                 "}")
            });
        }

        _dbContext.SaveChanges();

        return Ok();
    }
}