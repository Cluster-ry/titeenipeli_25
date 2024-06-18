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
        Pixel[] pixels = _dbContext.Map
                                   .Include(pixel => pixel.User)
                                   .ThenInclude(user => user!.Guild)
                                   .OrderBy(pixel => pixel.Y).ToArray();

        int width = _dbContext.Map.Max(pixel => pixel.X) + 1;
        int height = _dbContext.Map.Max(pixel => pixel.Y) + 1;

        MapModel map = new MapModel
        {
            Pixels = new PixelModel[height][]
        };

        PixelModel[] mapRow = new PixelModel[width];
        int lastRow = 0;

        foreach (Pixel pixel in pixels)
        {
            if (pixel.Y != lastRow)
            {
                map.Pixels[pixel.Y] = mapRow;
                mapRow = new PixelModel[width];
                lastRow = pixel.Y;
            }

            PixelModel mapPixel = new PixelModel
            {
                Type = PixelTypeEnum.Normal,
                Owner = (GuildEnum?)pixel.User?.Guild.Color,
                // TODO: Verify owning status of pixel, this can be done when we get user information from JWT
                OwnPixel = pixel.User == testUser
            };

            mapRow[pixel.X] = mapPixel;
        }

        return Ok(map);
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