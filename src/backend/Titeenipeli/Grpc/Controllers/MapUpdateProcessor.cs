using System.Collections.Concurrent;
using GrpcGeneratedServices;
using Titeenipeli.Enums;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Grpc.Common;
using Titeenipeli.Models;
using Titeenipeli.Options;
using Titeenipeli.Schema;

namespace Titeenipeli.Grpc.Controllers;

public class MapUpdateProcessor
{
    private readonly GrpcMapChangesInput _mapChangesInput;
    private readonly User _user;
    private readonly ConcurrentDictionary<int, IGrpcConnection<IncrementalMapUpdateResponse>> _grpcConnections;
    private readonly GameOptions _gameOptions;
    private int FogOfWarDistance => _gameOptions.FogOfWarDistance;

    private VisibilityMap _visibilityMap;

    private Dictionary<Coordinate, IncrementalMapUpdateResponse.Types.IncrementalMapUpdate> _pixelWireChanges = [];

    public MapUpdateProcessor(GrpcMapChangesInput mapChangesInput, ConcurrentDictionary<int, IGrpcConnection<IncrementalMapUpdateResponse>> connections, GameOptions gameOptions)
    {
        _mapChangesInput = mapChangesInput;
        _gameOptions = gameOptions;
        _grpcConnections = connections;
        _visibilityMap = new VisibilityMap(gameOptions.Width, gameOptions.Height);

        var connection = connections.FirstOrDefault();
        _user = connection.Value.User;
    }

    public async Task Process()
    {
        if (_user == null)
        {
            return;
        }

        ComputeVisibilityMap();
        ComputeUserLosses();
        ComputeNormalPixelChanges();
        ComputeUserWins();
        await SendResults();
    }

    private void ComputeVisibilityMap()
    {
        var pixelsOwnedByPlayer = _mapChangesInput.NewPixels.Where((pixel) => pixel.Value.User?.Id == _user.Id);
        foreach (var pixelOwnedByPlayer in pixelsOwnedByPlayer)
        {
            LoopNearbyPixelsInsideFieldOfView(
                _visibilityMap.SetVisible,
                pixelOwnedByPlayer.Value.Coordinate
            );
        }
    }

    private void ComputeUserLosses()
    {
        var changesCausingLosses = _mapChangesInput.Changes.Where((change) => change.OldOwner?.Id == _user?.Id);
        foreach (var change in changesCausingLosses)
        {
            ComputeSurroundingPixelVisibility(change);
        }
    }

    private void ComputeNormalPixelChanges()
    {
        IEnumerable<GrpcMapChangeInput> normalChanges = _mapChangesInput.Changes.Where((change) => change.OldOwner?.Id != _user?.Id && change.NewOwner?.Id != _user?.Id);
        foreach (var change in normalChanges)
        {
            bool insideFieldOfView = _visibilityMap.GetVisibility(change.Coordinate);
            if (insideFieldOfView)
            {
                AddStandardChange(change.Coordinate);
            }
        }
    }

    private void ComputeUserWins()
    {
        IEnumerable<GrpcMapChangeInput> winChanges = _mapChangesInput.Changes.Where((change) => change.NewOwner?.Id == _user?.Id);
        foreach (var change in winChanges)
        {
            LoopNearbyPixelsInsideFieldOfView(
                AddStandardChange,
                change.Coordinate
            );
        }
    }

    /// <summary>
    /// Computes player visibility losses when loosing pixel.
    /// Visibility for nearby pixels is lost, if visibility
    /// is not supported by other nearby pixels.
    /// </summary>
    private void ComputeSurroundingPixelVisibility(GrpcMapChangeInput change)
    {
        LoopNearbyPixelsInsideFieldOfView(
            (coordinate) =>
            {
                bool insideFieldOfView = _visibilityMap.GetVisibility(coordinate);
                if (!insideFieldOfView)
                {
                    AddEmptyingChange(coordinate);
                }
            },
            change.Coordinate
        );
    }

    private void LoopNearbyPixelsInsideFieldOfView(Action<Coordinate> action, Coordinate aroundCoordinate)
    {
        Coordinate coordinate;
        int minY = aroundCoordinate.Y - FogOfWarDistance;
        int maxY = aroundCoordinate.Y + FogOfWarDistance;
        int minX = aroundCoordinate.X - FogOfWarDistance;
        int maxX = aroundCoordinate.X + FogOfWarDistance;
        for (coordinate.Y = minY; coordinate.Y <= maxY; coordinate.Y++)
        {
            for (coordinate.X = minX; coordinate.X <= maxX; coordinate.X++)
            {
                action(coordinate);
            }
        }
    }

    private void AddEmptyingChange(Coordinate coordinate)
    {
        Coordinate spawnRelativeCoordinate = coordinate.ToSpawnRelativeCoordinate(_user);
        PixelTypes type = IsInsideMap(coordinate) ? PixelTypes.Normal : PixelTypes.MapBorder;

        IncrementalMapUpdateResponse.Types.IncrementalMapUpdate mapUpdate = new()
        {
            SpawnRelativeCoordinate = new()
            {
                X = spawnRelativeCoordinate.X,
                Y = spawnRelativeCoordinate.Y
            },
            Type = type,
            Owner = PixelOwners.Nobody,
            OwnPixel = false
        };
        _pixelWireChanges[coordinate] = mapUpdate;
    }

    private void AddStandardChange(Coordinate coordinate)
    {
        Coordinate spawnRelativeCoordinate = coordinate.ToSpawnRelativeCoordinate(_user);

        GrpcChangePixel newPixel;
        var foundValue = _mapChangesInput.NewPixels.TryGetValue(coordinate, out newPixel);
        if (!foundValue)
        {
            throw new Exception("Unable to add new standard change, missing pixel data.");
        }

        bool isNewSpawn = coordinate.X == newPixel.User?.SpawnX && coordinate.Y == newPixel.User?.SpawnY;
        PixelTypes type = isNewSpawn ? PixelTypes.Spawn : PixelTypes.Normal;
        PixelOwners owner = ConvertGuildToPixelOwners(newPixel.User?.Guild?.Name);
        bool ownPixel = newPixel.User?.Id == _user?.Id;

        IncrementalMapUpdateResponse.Types.IncrementalMapUpdate mapUpdate = new()
        {
            SpawnRelativeCoordinate = new()
            {
                X = spawnRelativeCoordinate.X,
                Y = spawnRelativeCoordinate.Y
            },
            Type = type,
            Owner = owner,
            OwnPixel = ownPixel
        };
        _pixelWireChanges[coordinate] = mapUpdate;
    }

    private static PixelOwners ConvertGuildToPixelOwners(GuildName? guildName)
    {
        switch (guildName)
        {
            case GuildName.Tietokilta:
                return PixelOwners.Tietokilta;
            case GuildName.Algo:
                return PixelOwners.Algo;
            case GuildName.Cluster:
                return PixelOwners.Cluster;
            case GuildName.OulunTietoteekkarit:
                return PixelOwners.OulunTietoteekkarit;
            case GuildName.TietoTeekkarikilta:
                return PixelOwners.TietoTeekkarikilta;
            case GuildName.Digit:
                return PixelOwners.Digit;
            case GuildName.Datateknologerna:
                return PixelOwners.Datateknologerna;
            case GuildName.Sosa:
                return PixelOwners.Sosa;
            default:
                return PixelOwners.Nobody;
        };
    }

    private bool IsInsideMap(Coordinate coordinate)
    {
        return coordinate.X >= 0 && coordinate.X < _gameOptions.Width && coordinate.Y >= 0 && coordinate.Y < _gameOptions.Height;
    }

    private async Task SendResults()
    {
        var incrementalResponse = new IncrementalMapUpdateResponse();
        foreach (var pixelChanges in _pixelWireChanges)
        {
            incrementalResponse.Updates.Add(pixelChanges.Value);
        }

        foreach (var connectionKeyValuePair in _grpcConnections)
        {
            var connection = connectionKeyValuePair.Value;
            await connection.ResponseStreamQueue.Writer.WriteAsync(incrementalResponse);
        }
    }
}
