using System.Collections.Generic;
using System.Text;
using Titeenipeli.Enums;
using Titeenipeli.Models;
using Titeenipeli.Schema;

namespace Titeenipeli.Tests.GameLogic;

using GuildPixel = (GuildName? guild, bool isSpawn);

public static class MapUtils
{
    private static readonly Dictionary<GuildName, int> _colourMappings = new()
    {
        { GuildName.Cluster, 31 },
        { GuildName.Tietokilta, 30 },
        { GuildName.Algo, 32 },
        { GuildName.OulunTietoteekkarit, 33 },
        { GuildName.TietoTeekkarikilta, 35 },
        { GuildName.Digit, 36 },
        { GuildName.Datateknologerna, 37 }
    };

    public static PixelWithType[,] BuildMapFromOwnerMatrix(GuildPixel[,] owners)
    {
        var ySize = owners.GetUpperBound(0) + 1;
        var xSize = owners.GetUpperBound(1) + 1;

        var map = BuildMapBorders(xSize, ySize);

        for (var y = 1; y < ySize + 1; y++)
        {
            for (var x = 1; x < xSize + 1; x++)
            {
                var pixelData = owners[y - 1, x - 1];
                map[x, y] = new PixelWithType
                {
                    Type = pixelData.isSpawn ? PixelType.Spawn : PixelType.Normal,
                    Owner = GuildNameToUser(pixelData.guild)
                };
            }
        }

        return map;
    }

    public static PixelWithType[,] BuildMapFromSpawnPointList(int size, (int x, int y, GuildName owner)[] spawnPoints)
    {
        var map = BuildMapBorders(size, size);

        for (var y = 1; y < size + 1; y++)
        {
            for (var x = 1; x < size + 1; x++)
            {
                map[x, y] = new PixelWithType
                {
                    Type = PixelType.Normal,
                    Owner = null
                };
            }
        }

        foreach (var (x, y, owner) in spawnPoints)
        {
            map[x, y] = new PixelWithType { Type = PixelType.Spawn, Owner = GuildNameToUser(owner) };
        }

        return map;
    }

    private static PixelWithType[,] BuildMapBorders(int xSize, int ySize)
    {
        var map = new PixelWithType[xSize + 2, ySize + 2];
        for (var x = 0; x < xSize + 2; x++)
        {
            map[x, 0] = new PixelWithType { Type = PixelType.MapBorder };
        }

        for (var y = 1; y < ySize + 1; y++)
        {
            map[0, y] = new PixelWithType { Type = PixelType.MapBorder };
            map[xSize + 1, y] = new PixelWithType { Type = PixelType.MapBorder };
        }

        for (var x = 0; x < xSize + 2; x++)
        {
            map[x, ySize + 1] = new PixelWithType { Type = PixelType.MapBorder };
        }

        return map;
    }

    public static User GuildNameToUser(GuildName? guildName)
    {
        Guild guild = guildName != null ? new() { Name = (GuildName)guildName } : null;
        return new()
        {
            Guild = guild,

            Code = "",
            SpawnX = 0,
            SpawnY = 0,
            LastPlacement = new(),
            PixelBucket = 0,
            TelegramId = "",
            FirstName = "",

            LastName = "",
            Username = "",
        };
    }

    public static string MapAsColours(PixelWithType[,] map)
    {
        var xSize = map.GetUpperBound(0) + 1;
        var ySize = map.GetUpperBound(1) + 1;
        var builder = new StringBuilder();
        for (var y = 0; y < ySize; y++)
        {
            for (var x = 0; x < xSize; x++)
            {
                var pixelOwner = map[x, y].Owner;
                if (pixelOwner is null || pixelOwner.Guild is null)
                {
                    builder.Append(' ');
                    continue;
                }

                // For some reason static analysis doesn't recognize the null guard just above ... oh well
                builder.Append($"\x1b[{_colourMappings[pixelOwner.Guild.Name]}m\u2588\x1b[0m");
            }

            builder.Append('\n');
        }

        return builder.ToString();
    }

    public static string MapAsNumbers(PixelWithType[,] map)
    {
        var xSize = map.GetUpperBound(0) + 1;
        var ySize = map.GetUpperBound(1) + 1;
        var builder = new StringBuilder();
        for (var y = 0; y < ySize; y++)
        {
            for (var x = 0; x < xSize; x++)
            {
                var pixelOwner = map[x, y].Owner;
                if (pixelOwner is null || pixelOwner.Guild is null)
                {
                    builder.Append(' ');
                    continue;
                }

                // For some reason static analysis doesn't recognize the null guard just above ... oh well
                builder.Append((int)pixelOwner.Guild.Name);
            }

            builder.Append('\n');
        }

        return builder.ToString();
    }
}