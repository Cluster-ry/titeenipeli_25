using System.Threading.Channels;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Grpc.Controllers;
using Titeenipeli.Options;

namespace Titeenipeli.Grpc.Services;

public class IncrementalMapUpdateCoreService : GrpcService, IIncrementalMapUpdateCoreService
{
    private const int MaxChannelSize = 100;

    private readonly ILogger<IncrementalMapUpdateService> _logger;
    private readonly GameOptions _gameOptions;

    private readonly Channel<GrpcMapChangesInput> _mapChangeQueue =
        Channel.CreateBounded<GrpcMapChangesInput>(MaxChannelSize);

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
        updateTasks.AddRange(Connections
                             .Select(connectionKeyValuePair => new MapUpdateProcessor(this,
                                 mapChangesInput,
                                 connectionKeyValuePair.Value, _gameOptions))
                             .Select(mapUpdateProcessor => Task.Run(mapUpdateProcessor.Process)));

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