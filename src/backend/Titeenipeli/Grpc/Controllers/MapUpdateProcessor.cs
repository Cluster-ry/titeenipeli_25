using System.Collections.Concurrent;
using GrpcGeneratedServices;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Models;
using Titeenipeli.Schema;

namespace Titeenipeli.Grpc.Common;

public class MapUpdateProcessor
{
    private readonly GrpcMapChangesInput _mapChangesInput;
    private readonly User _user;
    private readonly ConcurrentDictionary<int, GrpcConnection<IncrementalMapUpdateResponse>> _grpcConnections;
    private readonly int _fogOfWarDistance;

    private Dictionary<Coordinate, IncrementalMapUpdateResponse.Types.IncrementalMapUpdate> _pixelWireChanges = [];

    public MapUpdateProcessor(GrpcMapChangesInput mapChangesInput, ConcurrentDictionary<int, GrpcConnection<IncrementalMapUpdateResponse>> connections, int fogOfWarDistance) {
        _mapChangesInput = mapChangesInput;
        _fogOfWarDistance = fogOfWarDistance;
        _grpcConnections = connections;

        var connection = connections.FirstOrDefault();
        _user = connection.Value.User;
    }

    public async void Process()
    {
        if (_user == null) {
            return;
        }

        ComputeUserLosses();
        await SendResults();
    }

    private void ComputeUserLosses()
    {
        foreach (var change in _mapChangesInput.Changes)
        {
            if (change.OldOwner?.Id != _user?.Id)
            {
                continue;
            }

            ComputeSurroundingPixelVisibility(change);
        }
    }

    /// <summary>
    /// Computes player visibility losses when loosing pixel.
    /// Visibility for nearby pixels is lost, if visibility
    /// is not supported by other nearby pixels.
    /// </summary>
    private void ComputeSurroundingPixelVisibility(GrpcMapChangeInput change)
    {
        Coordinate coordinate;
        for (coordinate.Y = change.Coordinate.Y - _fogOfWarDistance; coordinate.Y < change.Coordinate.Y + _fogOfWarDistance; coordinate.Y++)
        {
            for (coordinate.X = change.Coordinate.X - _fogOfWarDistance; coordinate.X < change.Coordinate.X + _fogOfWarDistance; coordinate.X++)
            {
                bool inside = IsInsideFieldOfView(change);
                if (!inside) {
                    AddNullifyingChange(coordinate);
                }
            }
        }
    }

    private bool IsInsideFieldOfView(GrpcMapChangeInput change) {
        Coordinate candidateCoordinate;
        for (candidateCoordinate.Y = change.Coordinate.Y - _fogOfWarDistance; candidateCoordinate.Y < change.Coordinate.Y + _fogOfWarDistance; candidateCoordinate.Y++)
        {
            for (candidateCoordinate.X = change.Coordinate.X - _fogOfWarDistance; candidateCoordinate.X < change.Coordinate.X + _fogOfWarDistance; candidateCoordinate.X++)
            {
                GrpcChangePixel candidatePixel;
                bool pixelExists = _mapChangesInput.NewPixels.TryGetValue(candidateCoordinate, out candidatePixel);
                if (pixelExists && candidatePixel.User?.Id == _user.Id) {
                    return true;
                }
            }
        }
        return false;
    }

    private void AddNullifyingChange(Coordinate coordinate) {
        var spawnRelativeCoordinate = coordinate.ToSpawnRelativeCoordinate(_user);
        IncrementalMapUpdateResponse.Types.IncrementalMapUpdate mapUpdate = new() {
            SpawnRelativeCoordinate = new() {
                X = spawnRelativeCoordinate.X,
                Y = spawnRelativeCoordinate.Y
            },
            Type = PixelTypes.Normal,
            Owner = PixelOwners.Nobody,
            OwnPixel = false
        };
        _pixelWireChanges.Add(coordinate, mapUpdate);
    }

    private async Task SendResults() {
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
