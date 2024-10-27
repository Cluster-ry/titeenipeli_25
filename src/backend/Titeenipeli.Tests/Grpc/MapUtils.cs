using System.Collections.Generic;
using Google.Protobuf.Collections;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Models;
using Titeenipeli.Options;
using Titeenipeli.Schema;
using static GrpcGeneratedServices.IncrementalMapUpdateResponse.Types;

namespace Titeenipeli.Tests.Grpc;

public static class MapUtils
{
    public static Dictionary<Coordinate, GrpcChangePixel> MatrixOfUsersToPixels(int[,] map, List<User> users)
    {
        int mapWidth = map.GetUpperBound(0);
        int mapHeight = map.GetUpperBound(1);

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

    public static int[,] GrpcUpdatesToUserMap(RepeatedField<IncrementalMapUpdate> updates, GameOptions gameOptions)
    {
        int fogOfWarDistance = gameOptions.FogOfWarDistance;
        int padding = fogOfWarDistance * 2;

        int[,] map = new int[gameOptions.Width + padding, gameOptions.Height + padding];
        for (int y = 0; y < gameOptions.Height + padding; y++)
        {
            for (int x = 0; x < gameOptions.Width + padding; x++)
            {
                map[x, y] = MapUpdateProcessorTest.Nop;
            }
        }

        foreach (IncrementalMapUpdate update in updates)
        {
            int userId;
            if (update.Guild == PixelGuild.Cluster)
            {
                userId = MapUpdateProcessorTest.Own;
            }
            else if (update.Guild == PixelGuild.Tietokilta)
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
