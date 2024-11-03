using System.Collections.Concurrent;
using System.Threading.Channels;
using GrpcGeneratedServices;
using Titeenipeli.Controllers.Grpc;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Grpc.Common;
using Titeenipeli.Grpc.Services;
using Titeenipeli.Options;
using Titeenipeli.Services;

namespace Titeenipeli.Services.Grpc;

public class IncrementalMapUpdateCoreService : GrpcService<IncrementalMapUpdateResponse>, IIncrementalMapUpdateCoreService
{
    private const int MaxChannelSize = 100;

    private readonly ILogger<StateUpdateService> _logger;
    private readonly GameOptions _gameOptions;

    private readonly Channel<GrpcMapChangesInput> _mapChangeQueue =
        Channel.CreateBounded<GrpcMapChangesInput>(MaxChannelSize);

    public IncrementalMapUpdateCoreService(
            ILogger<StateUpdateService> logger,
            GameOptions gameOptions,
            IBackgroundGraphicsService backgroundGraphicsService
        )
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

    private async void ProcessMapChangeRequests()
    {
        await foreach (var mapChangesInput in _mapChangeQueue.Reader.ReadAllAsync())
        {
            await GenerateAndRunMapUpdateTasks(mapChangesInput);
        }
    }

    private async Task GenerateAndRunMapUpdateTasks(GrpcMapChangesInput mapChangesInput)
    {
        List<Task> updateTasks = new(Connections.Count);
        foreach (KeyValuePair<int, ConcurrentDictionary<int, IGrpcConnection<IncrementalMapUpdateResponse>>>
                     connectionKeyValuePair in Connections)
        {
            MapUpdateProcessor mapUpdateProcessor =
                new(this, mapChangesInput, connectionKeyValuePair.Value, _gameOptions);

            var updateTask = Task.Run(mapUpdateProcessor.Process);
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