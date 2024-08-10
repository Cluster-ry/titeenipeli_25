using Grpc.Core;
using GrpcGeneratedServices;
using Microsoft.Extensions.Logging;
using static GrpcGeneratedServices.IncrementalMapUpdateResponse.Types;

namespace Titeenipeli.gRPC.Services;

public class IncrementalMapUpdateService(ILogger<IncrementalMapUpdateService> logger) : MapUpdate.MapUpdateBase
{
    private readonly ILogger<IncrementalMapUpdateService> _logger = logger;

    public override async Task GetIncremental(IncrementalMapUpdateRequest request, IServerStreamWriter<IncrementalMapUpdateResponse> responseStream, ServerCallContext context)
    {
        var random = new Random();

        while (!context.CancellationToken.IsCancellationRequested)
        {
            var incrementalMapUpdate = new IncrementalMapUpdate()
            {
                Type = PixelTypes.Normal,
                Owner = PixelOwners.Cluster,
                OwnPixel = true,
                SpawnRelativeCoordinate = new()
                {
                    X = random.Next(256),
                    Y = random.Next(256)
                }
            };

            var incrementalResponse = new IncrementalMapUpdateResponse();
            incrementalResponse.Updates.Add(incrementalMapUpdate);

            await responseStream.WriteAsync(incrementalResponse);
            _logger.LogDebug("Send incremental response: {incrementalResponse}", incrementalResponse);
            await Task.Delay(10 * 1000);
        }
    }
}
