using Titeenipeli.Grpc.ChangeEntities;
using GrpcGeneratedServices;
using System.Collections.Generic;
using Titeenipeli.Models;
using Google.Protobuf.Collections;
using Titeenipeli.Schema;
using Titeenipeli.Options;

namespace Titeenipeli.Tests.Grpc;

internal static class MapUtils
{
    internal static Dictionary<Coordinate, GrpcChangePixel> MatrixOfUsersToPixels(int[,] map, List<User> users)
    {
        var mapWidth = map.GetUpperBound(0);
        var mapHeight = map.GetUpperBound(1);

        Dictionary<Coordinate, GrpcChangePixel> pixels = [];
        Coordinate coordinate = new();
        for (coordinate.Y = 0; coordinate.Y < mapHeight; coordinate.Y++)
        {
            for (coordinate.X = 0; coordinate.X < mapWidth; coordinate.X++)
            {
                GrpcChangePixel pixel;
                int mapUser = map[coordinate.Y, coordinate.X];
                if (mapUser == 0)
                {
                    pixel = new GrpcChangePixel(coordinate, null);
                }
                else
                {
                    pixel = new GrpcChangePixel(coordinate, users[mapUser - 1]);
                }
                pixels.Add(coordinate, pixel);
            }
        }
        return pixels;
    }

    internal static int[,] GrpcUpdatesToUserMap(RepeatedField<IncrementalMapUpdateResponse.Types.IncrementalMapUpdate> updates, GameOptions gameOptions)
    {
        var fogOfWarDistance = gameOptions.FogOfWarDistance;
        var padding = fogOfWarDistance * 2;

        int[,] map = new int[gameOptions.Width + padding, gameOptions.Height + padding];
        for (int y = 0; y < gameOptions.Height + padding; y++)
        {
            for (int x = 0; x < gameOptions.Width + padding; x++)
            {
                map[x, y] = MapUpdateProcessorTest.Nop;
            }
        }

        foreach (var update in updates)
        {
            int userId;
            if (update.Owner == PixelOwners.Cluster)
            {
                userId = MapUpdateProcessorTest.Own;
            }
            else if (update.Owner == PixelOwners.Tietokilta)
            {
                userId = MapUpdateProcessorTest.Oth;
            }
            else if (update.Type == PixelTypes.MapBorder)
            {
                userId = MapUpdateProcessorTest.Bor;
            }
            else
            {
                userId = MapUpdateProcessorTest.Emp;
            }
            map[update.SpawnRelativeCoordinate.Y + fogOfWarDistance, update.SpawnRelativeCoordinate.X + fogOfWarDistance] = userId;
        }
        return map;
    }
}
