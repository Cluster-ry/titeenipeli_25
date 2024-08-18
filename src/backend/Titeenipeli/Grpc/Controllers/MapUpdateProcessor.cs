using System.Collections.Concurrent;
using GrpcGeneratedServices;
using Titeenipeli.Enums;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Grpc.Common;
using Titeenipeli.Grpc.Services;
using Titeenipeli.Models;
using Titeenipeli.Options;
using Titeenipeli.Schema;
using static GrpcGeneratedServices.IncrementalMapUpdateResponse.Types;

namespace Titeenipeli.Grpc.Controllers;

public class MapUpdateProcessor
{
    private readonly IIncrementalMapUpdateCoreService _incrementalMapUpdateCoreService;

    private readonly GrpcMapChangesInput _mapChangesInput;
    private readonly User _user;
    private readonly ConcurrentDictionary<int, IGrpcConnection<IncrementalMapUpdateResponse>> _grpcConnections;
    private readonly GameOptions _gameOptions;

    private readonly Dictionary<Coordinate, IncrementalMapUpdate> _pixelWireChanges = [];

    private VisibilityMap _visibilityMap;

    public MapUpdateProcessor(
            IIncrementalMapUpdateCoreService incrementalMapUpdateCoreService,
            GrpcMapChangesInput mapChangesInput,
            ConcurrentDictionary<int, IGrpcConnection<IncrementalMapUpdateResponse>> connections,
            GameOptions gameOptions
        )
    {
        _incrementalMapUpdateCoreService = incrementalMapUpdateCoreService;
        _mapChangesInput = mapChangesInput;
        _gameOptions = gameOptions;
        _grpcConnections = connections;
        _visibilityMap = new VisibilityMap(gameOptions.Width, gameOptions.Height, gameOptions.FogOfWarDistance);

        KeyValuePair<int, IGrpcConnection<IncrementalMapUpdateResponse>> connection = connections.FirstOrDefault();
        _user = connection.Value.User;
    }

    public async Task Process()
    {
        if (_user == null)
        {
            return;
        }

        try
        {
            ComputeVisibilityMap();
            ComputeUserLosses();
            ComputeNormalPixelChanges();
            ComputeUserWins();
            await SendResults();
        }
        catch (Exception)
        {
            foreach (KeyValuePair<int, IGrpcConnection<IncrementalMapUpdateResponse>> connectionKeyValue in _grpcConnections)
            {
                _incrementalMapUpdateCoreService.RemoveGrpcConnection(connectionKeyValue.Value);
            }
            throw;
        }
    }

    private void ComputeVisibilityMap()
    {
        IEnumerable<KeyValuePair<Coordinate, GrpcChangePixel>> pixelsOwnedByPlayer =
            _mapChangesInput.NewPixels.Where((pixel) => pixel.Value.User?.Id == _user.Id);
        foreach (KeyValuePair<Coordinate, GrpcChangePixel> pixelOwnedByPlayer in pixelsOwnedByPlayer)
        {
            LoopNearbyPixelsInsideFogOfWar(
                _visibilityMap.SetVisible,
                pixelOwnedByPlayer.Value.Coordinate
            );
        }
    }

    private void ComputeUserLosses()
    {
        IEnumerable<GrpcMapChangeInput> changesCausingLosses = _mapChangesInput.Changes.Where((change) =>
            change.OldOwner?.Id == _user?.Id);
        foreach (GrpcMapChangeInput change in changesCausingLosses)
        {
            ComputeSurroundingPixelVisibility(change);

            // Special case, if lost pixel remains within field of view, update its value.
            bool insideFogOfWar = _visibilityMap.GetVisibility(change.Coordinate);
            if (insideFogOfWar)
            {
                AddStandardChange(change.Coordinate);
            }
        }
    }

    private void ComputeNormalPixelChanges()
    {
        IEnumerable<GrpcMapChangeInput> normalChanges = _mapChangesInput.Changes.Where((change) =>
            change.OldOwner?.Id != _user?.Id && change.NewOwner?.Id != _user?.Id);
        foreach (GrpcMapChangeInput change in normalChanges)
        {
            bool insideFogOfWar = _visibilityMap.GetVisibility(change.Coordinate);
            if (insideFogOfWar)
            {
                AddStandardChange(change.Coordinate);
            }
        }
    }

    private void ComputeUserWins()
    {
        IEnumerable<GrpcMapChangeInput> winChanges = _mapChangesInput.Changes.Where((change) =>
            change.NewOwner?.Id == _user?.Id);
        foreach (GrpcMapChangeInput change in winChanges)
        {
            LoopNearbyPixelsInsideFogOfWar(
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
        LoopNearbyPixelsInsideFogOfWar(
            AddNotOwnedChangeIfNotInsideFogOfWar,
            change.Coordinate
        );
    }

    private void AddNotOwnedChangeIfNotInsideFogOfWar(Coordinate coordinate)
    {
        bool insideFogOfWar = _visibilityMap.GetVisibility(coordinate);
        if (!insideFogOfWar)
        {
            AddNotOwnedChange(coordinate, PixelTypes.Normal);
        }
    }

    private void LoopNearbyPixelsInsideFogOfWar(Action<Coordinate> action, Coordinate aroundCoordinate)
    {
        int fogOfWarDistance = _gameOptions.FogOfWarDistance;
        int minY = aroundCoordinate.Y - fogOfWarDistance;
        int maxY = aroundCoordinate.Y + fogOfWarDistance;
        int minX = aroundCoordinate.X - fogOfWarDistance;
        int maxX = aroundCoordinate.X + fogOfWarDistance;

        Coordinate coordinate = new();
        for (coordinate.Y = minY; coordinate.Y <= maxY; coordinate.Y++)
        {
            for (coordinate.X = minX; coordinate.X <= maxX; coordinate.X++)
            {
                action(coordinate);
            }
        }
    }

    private void AddStandardChange(Coordinate coordinate)
    {
        if (!IsInsideMap(coordinate))
        {
            AddNotOwnedChange(coordinate, PixelTypes.MapBorder);
            return;
        }

        Coordinate spawnRelativeCoordinate = coordinate.ToSpawnRelativeCoordinate(_user);

        GrpcChangePixel newPixel;
        bool foundValue = _mapChangesInput.NewPixels.TryGetValue(coordinate, out newPixel);
        if (!foundValue)
        {
            throw new Exception("Unable to add new standard change, missing pixel data.");
        }

        bool isNewSpawn = coordinate.X == newPixel.User?.SpawnX && coordinate.Y == newPixel.User?.SpawnY;
        PixelTypes type = isNewSpawn ? PixelTypes.Spawn : PixelTypes.Normal;
        PixelOwners owner = ConvertGuildToPixelOwners(newPixel.User?.Guild?.Name);
        bool ownPixel = newPixel.User?.Id == _user?.Id;

        IncrementalMapUpdate mapUpdate = new()
        {
            SpawnRelativeCoordinate = new RelativeCoordinate()
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

    private void AddNotOwnedChange(Coordinate coordinate, PixelTypes pixelType)
    {
        Coordinate spawnRelativeCoordinate = coordinate.ToSpawnRelativeCoordinate(_user);

        IncrementalMapUpdate mapUpdate = new()
        {
            SpawnRelativeCoordinate = new()
            {
                X = spawnRelativeCoordinate.X,
                Y = spawnRelativeCoordinate.Y
            },
            Type = pixelType,
            Owner = PixelOwners.Nobody,
            OwnPixel = false
        };
        _pixelWireChanges[coordinate] = mapUpdate;
    }

    private static PixelOwners ConvertGuildToPixelOwners(GuildName? guildName)
    {
        bool success = Enum.TryParse(guildName.ToString(), false, out PixelOwners result);
        return success ? result : PixelOwners.Nobody;
    }

    private bool IsInsideMap(Coordinate coordinate)
    {
        return coordinate.X >= 0 && coordinate.X < _gameOptions.Width && coordinate.Y >= 0 && coordinate.Y < _gameOptions.Height;
    }

    private async Task SendResults()
    {
        if (_pixelWireChanges.Count == 0)
        {
            return;
        }

        IncrementalMapUpdateResponse incrementalResponse = new();
        foreach (KeyValuePair<Coordinate, IncrementalMapUpdate> pixelChanges in _pixelWireChanges)
        {
            incrementalResponse.Updates.Add(pixelChanges.Value);
        }

        foreach (KeyValuePair<int, IGrpcConnection<IncrementalMapUpdateResponse>> connectionKeyValuePair in _grpcConnections)
        {
            IGrpcConnection<IncrementalMapUpdateResponse> connection = connectionKeyValuePair.Value;
            try
            {
                await connection.ResponseStreamQueue.Writer.WriteAsync(incrementalResponse);
            }
            catch (Exception)
            {
                _incrementalMapUpdateCoreService.RemoveGrpcConnection(connection);
            }
        }
    }
}
