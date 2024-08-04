using Grpc.Core;
using GrpcGeneratedServices;
using Titeenipeli.Extensions;
using Titeenipeli.Grpc.Common;
using Titeenipeli.Schema;
using Titeenipeli.Services;
using Titeenipeli.Services.RepositoryServices.Interfaces;

namespace Titeenipeli.Grpc.Services;

public class IncrementalMapUpdateService(ILogger<IncrementalMapUpdateService> logger, JwtService jwtService, IUserRepositoryService userRepositoryService, IIncrementalMapUpdateCoreService incrementalMapUpdateCoreService) : MapUpdate.MapUpdateBase
{
    private readonly ILogger<IncrementalMapUpdateService> _logger = logger;
    private readonly JwtService _jwtService = jwtService;
    private readonly IUserRepositoryService _userRepositoryService = userRepositoryService;
    private readonly IIncrementalMapUpdateCoreService _incrementalMapUpdateCoreService = incrementalMapUpdateCoreService;

    public override async Task GetIncremental(IncrementalMapUpdateRequest request, IServerStreamWriter<IncrementalMapUpdateResponse> responseStream, ServerCallContext context)
    {
        var httpContext = context.GetHttpContext();

        var jwtClaim = httpContext.GetUser(_jwtService);
        if (jwtClaim == null)
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, "Missing or invalid authentication cookie."));
        }

        User? user = _userRepositoryService.GetById(jwtClaim.Id);
        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, "User couldn't be found."));
        }

        var grpcConnection = new GrpcConnection<IncrementalMapUpdateResponse>(user, responseStream);
        _incrementalMapUpdateCoreService.AddGrpcConnection(grpcConnection);

        var incrementalResponse = new IncrementalMapUpdateResponse();
        await responseStream.WriteAsync(incrementalResponse);

        await Task.Delay(Timeout.Infinite, context.CancellationToken);

        /*
        var random = new Random();

        while (true)
        {
            var incrementalMapUpdate = new IncrementalMapUpdate()
            {
                Type = PixelTypes.Normal,
                Owner = PixelOwners.Cluster,
                OwnPixel = true,
                SpawnRelativeCoordinate = new()
                {
                    X = Convert.ToInt32(random.NextInt64(256)),
                    Y = Convert.ToInt32(random.NextInt64(256))
                }
            };

            var incrementalResponse = new IncrementalMapUpdateResponse();
            incrementalResponse.Updates.Add(incrementalMapUpdate);

            await responseStream.WriteAsync(incrementalResponse);
            _logger.LogDebug("Send incremental response: {incrementalResponse}", incrementalResponse);
            Thread.Sleep(10 * 1000);
        }
        */
    }
}
