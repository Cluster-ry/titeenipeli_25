using System.Collections.Concurrent;
using System.Threading.Channels;
using GrpcGeneratedServices;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Grpc.Common;
using Titeenipeli.Grpc.Controllers;
using Titeenipeli.Options;

namespace Titeenipeli.Grpc.Services;

public class IncrementalMapUpdateCoreService : IIncrementalMapUpdateCoreService
{
    private const int _maxChannelSize = 100;
    private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, IGrpcConnection<IncrementalMapUpdateResponse>>> _connections = new();

    private readonly ILogger<IncrementalMapUpdateService> _logger;
    private readonly GameOptions _gameOptions;

    private readonly Channel<GrpcMapChangesInput> _mapChangeQueue = Channel.CreateBounded<GrpcMapChangesInput>(_maxChannelSize);

    public IncrementalMapUpdateCoreService(ILogger<IncrementalMapUpdateService> logger, GameOptions gameOptions)
    {
        _logger = logger;
        _gameOptions = gameOptions;
        Task.Run(ProcessMapChangeRequests);
    }

    public async void UpdateUsersMapState(GrpcMapChangesInput mapChangesInput)
    {
        await _mapChangeQueue.Writer.WaitToWriteAsync();
        await _mapChangeQueue.Writer.WriteAsync(mapChangesInput);
    }

    public void AddGrpcConnection(IGrpcConnection<IncrementalMapUpdateResponse> connection)
    {
        ConcurrentDictionary<int, IGrpcConnection<IncrementalMapUpdateResponse>> userConnections =
            _connections.GetOrAdd(connection.User.Id, new ConcurrentDictionary<int, IGrpcConnection<IncrementalMapUpdateResponse>>());
        userConnections.TryAdd(connection.Id, connection);
    }

    public void RemoveGrpcConnection(IGrpcConnection<IncrementalMapUpdateResponse> connection)
    {
        ConcurrentDictionary<int, IGrpcConnection<IncrementalMapUpdateResponse>>? dictionaryOutput;
        bool retrievalSucceeded = _connections.TryGetValue(connection.User.Id, out dictionaryOutput);
        if (retrievalSucceeded)
        {
            connection.Dispose();
            dictionaryOutput?.TryRemove(connection.Id, out _);
        }
    }

    private async void ProcessMapChangeRequests()
    {
        await foreach (GrpcMapChangesInput mapChangesInput in _mapChangeQueue.Reader.ReadAllAsync())
        {
            await GenerateAndRunMapUpdateTasks(mapChangesInput);
        }
    }

    private async Task GenerateAndRunMapUpdateTasks(GrpcMapChangesInput mapChangesInput)
    {
        List<Task> updateTasks = new(_connections.Count);
        foreach (KeyValuePair<int, ConcurrentDictionary<int, IGrpcConnection<IncrementalMapUpdateResponse>>> connectionKeyValuePair in _connections)
        {
            MapUpdateProcessor mapUpdateProcessor = new(this, mapChangesInput, connectionKeyValuePair.Value, _gameOptions);
            Task updateTask = Task.Run(mapUpdateProcessor.Process);
            updateTasks.Add(updateTask);
        }

        try
        {
            await Task.WhenAll(updateTasks);
        }
        catch (Exception exception)
        {
            _logger.LogError("Error while processing gRPC updates: {error}", exception);
        }
    }
}
