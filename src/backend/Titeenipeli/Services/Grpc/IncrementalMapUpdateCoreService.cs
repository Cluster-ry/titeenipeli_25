using System.Threading.Channels;
using GrpcGeneratedServices;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Controllers.Grpc;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Grpc.Services;
using Titeenipeli.Options;

namespace Titeenipeli.Services.Grpc;

public class IncrementalMapUpdateCoreService : GrpcService<IncrementalMapUpdateResponse>, IIncrementalMapUpdateCoreService
{
    private const int MaxChannelSize = 100;

    private readonly ILogger<StateUpdateService> _logger;
    private readonly GameOptions _gameOptions;
    private readonly IBackgroundGraphicsService _backgroundGraphicsService;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly Channel<GrpcMapChangesInput> _mapChangeQueue =
        Channel.CreateBounded<GrpcMapChangesInput>(MaxChannelSize);

    public IncrementalMapUpdateCoreService(
            ILogger<StateUpdateService> logger,
            GameOptions gameOptions,
            IBackgroundGraphicsService backgroundGraphicsService,
            IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _gameOptions = gameOptions;
        _backgroundGraphicsService = backgroundGraphicsService;
        _serviceScopeFactory = serviceScopeFactory;
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
        var guildRepositoryService = _serviceScopeFactory.CreateScope().ServiceProvider
                                                         .GetRequiredService<IGuildRepositoryService>();

        var guilds = guildRepositoryService.GetAll();

        List<Task> updateTasks = new(Connections.Count);
        foreach (var connectionKeyValuePair in Connections)
        {
            var user = connectionKeyValuePair.Value.FirstOrDefault().Value.User;
            user.Guild = guilds.FirstOrDefault(guild => guild.Id == user.Guild.Id)!;

            MapUpdateProcessor mapUpdateProcessor =
                new(this, mapChangesInput, connectionKeyValuePair.Value, _gameOptions, _backgroundGraphicsService,
                    user);

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