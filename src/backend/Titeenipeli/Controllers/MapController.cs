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

        MapModel map = new MapModel{Pixels = new PixelModel[height, width]};

        foreach (Pixel pixel in pixels)
        {
            PixelModel mapPixel = new PixelModel
            {
                Type = PixelTypeEnum.Normal,
                Owner = (GuildEnum?)pixel.User?.Guild.Color,
                // TODO: Verify owning status of pixel, this can be done when we get user information from JWT
                OwnPixel = pixel.User == testUser
            };

            map.Pixels[pixel.Y, pixel.X] = mapPixel;
        }

        return Ok(map);
    }
}