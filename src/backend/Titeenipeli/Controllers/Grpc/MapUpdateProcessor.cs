using System.Collections.Concurrent;
using GrpcGeneratedServices;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Grpc.Common;
using Titeenipeli.Options;
using Titeenipeli.Services.Grpc;
using static GrpcGeneratedServices.IncrementalMapUpdateResponse.Types;

namespace Titeenipeli.Controllers.Grpc;

public class MapUpdateProcessor
{
    private readonly IIncrementalMapUpdateCoreService _incrementalMapUpdateCoreService;

    private readonly GrpcMapChangesInput _mapChangesInput;
    private readonly User? _user;
    private readonly ConcurrentDictionary<int, IGrpcConnection<IncrementalMapUpdateResponse>> _grpcConnections;
    private readonly GameOptions _gameOptions;

    private readonly Dictionary<Coordinate, IncrementalMapUpdate> _pixelWireChanges = [];

    private readonly VisibilityMap _visibilityMap;

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
        // User can be null if user does not have any active connections.
        // Must be checked here to prevent potential unnecessary calculations.
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
            _mapChangesInput.NewPixels.Where((pixel) => pixel.Value.User?.Id == _user?.Id);
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
        IEnumerable<MapChange> changesCausingLosses = _mapChangesInput.Changes.Where((change) =>
            change.OldOwner?.Id == _user?.Id);
        foreach (MapChange change in changesCausingLosses)
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
        IEnumerable<MapChange> normalChanges = _mapChangesInput.Changes.Where((change) =>
            change.OldOwner?.Id != _user?.Id && change.NewOwner?.Id != _user?.Id);
        foreach (MapChange change in normalChanges)
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
        IEnumerable<MapChange> winChanges = _mapChangesInput.Changes.Where((change) =>
            change.NewOwner?.Id == _user?.Id);
        foreach (MapChange change in winChanges)
        {
            LoopNearbyPixelsInsideFogOfWar(
                AddStandardChange,
                change.Coordinate
            );
        }
    }

    private void ComputeSurroundingPixelVisibility(MapChange change)
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
            AddNotOwnedChange(coordinate, PixelTypes.FogOfWar);
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

        var spawnRelativeCoordinate = ToSpawnRelativeCoordinate(coordinate, _user);

        GrpcChangePixel newPixel;
        bool foundValue = _mapChangesInput.NewPixels.TryGetValue(coordinate, out newPixel);
        if (!foundValue)
        {
            throw new Exception("Unable to add new standard change, missing pixel data.");
        }

        bool isNewSpawn = coordinate.X == newPixel.User?.SpawnX && coordinate.Y == newPixel.User?.SpawnY;
        PixelTypes type = isNewSpawn ? PixelTypes.Spawn : PixelTypes.Normal;
        PixelGuild guild = ConvertGuildToPixelGuild(newPixel.User?.Guild?.Name);
        PixelUser owner = new PixelUser() { Id = _user?.Id ?? 0 };

        IncrementalMapUpdate mapUpdate = new()
        {
            SpawnRelativeCoordinate = new RelativeCoordinate()
            {
                X = spawnRelativeCoordinate.X,
                Y = spawnRelativeCoordinate.Y
            },
            Type = type,
            Guild = guild,
            Owner = owner,
        };
        _pixelWireChanges[coordinate] = mapUpdate;
    }

    private void AddNotOwnedChange(Coordinate coordinate, PixelTypes pixelType)
    {
        var spawnRelativeCoordinate = ToSpawnRelativeCoordinate(coordinate, _user);

        IncrementalMapUpdate mapUpdate = new()
        {
            SpawnRelativeCoordinate = new()
            {
                X = spawnRelativeCoordinate.X,
                Y = spawnRelativeCoordinate.Y
            },
            Type = pixelType,
            Guild = PixelGuild.Nobody,
            Owner = new PixelUser() { Id = 0 },
        };
        _pixelWireChanges[coordinate] = mapUpdate;
    }

    private static PixelGuild ConvertGuildToPixelGuild(GuildName? guildName)
    {
        bool success = Enum.TryParse(guildName.ToString(), false, out PixelGuild result);
        return success ? result : PixelGuild.Nobody;
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

    public Coordinate ToSpawnRelativeCoordinate(Coordinate coordinate, User? user)
    {
        if (user == null)
        {
            return coordinate;
        }

        return new Coordinate
        {
            X = coordinate.X - user.SpawnX,
            Y = coordinate.Y - user.SpawnY
        };
    }
}
