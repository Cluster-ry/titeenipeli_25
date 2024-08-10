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
        Coordinate coordinate;
        for (coordinate.Y = 0; coordinate.Y < mapHeight; coordinate.Y++)
        {
            for (coordinate.X = 0; coordinate.X < mapWidth; coordinate.X++)
            {
                GrpcChangePixel pixel;
                int mapUser = map[coordinate.X, coordinate.Y];
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
        int[,] map = new int[gameOptions.Width, gameOptions.Height];
        foreach (var update in updates)
        {
            int userId;
            if (update.Owner == PixelOwners.Cluster)
            {
                userId = 1;
            }
            else if (update.Owner == PixelOwners.Tietokilta)
            {
                userId = 2;
            }
            else
            {
                userId = 0;
            }
            map[update.SpawnRelativeCoordinate.X, update.SpawnRelativeCoordinate.Y] = userId;
        }
        return map;
    }
}
