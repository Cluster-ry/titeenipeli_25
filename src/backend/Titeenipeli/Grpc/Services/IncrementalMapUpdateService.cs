using Grpc.Core;
using GrpcGeneratedServices;
using Titeenipeli.Extensions;
using Titeenipeli.Grpc.Common;
using Titeenipeli.Models;
using Titeenipeli.Schema;
using Titeenipeli.Services;
using Titeenipeli.Services.RepositoryServices.Interfaces;

namespace Titeenipeli.Grpc.Services;

public class IncrementalMapUpdateService(
        JwtService jwtService,
        IUserRepositoryService userRepositoryService,
        IIncrementalMapUpdateCoreService incrementalMapUpdateCoreService
    ) : MapUpdate.MapUpdateBase
{
    private readonly JwtService _jwtService = jwtService;
    private readonly IUserRepositoryService _userRepositoryService = userRepositoryService;
    private readonly IIncrementalMapUpdateCoreService _incrementalMapUpdateCoreService = incrementalMapUpdateCoreService;

    public override async Task GetIncremental(IncrementalMapUpdateRequest request, IServerStreamWriter<IncrementalMapUpdateResponse> responseStream, ServerCallContext context)
    {
        HttpContext httpContext = context.GetHttpContext();

        JwtClaim? jwtClaim = httpContext.GetUser(_jwtService);
        if (jwtClaim == null)
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, "Missing or invalid authentication cookie."));
        }

        User? user = _userRepositoryService.GetById(jwtClaim.Id);
        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, "User couldn't be found."));
        }

        GrpcConnection<IncrementalMapUpdateResponse> grpcConnection =
            new(user, responseStream, _incrementalMapUpdateCoreService.RemoveGrpcConnection);
        _incrementalMapUpdateCoreService.AddGrpcConnection(grpcConnection);

        IncrementalMapUpdateResponse incrementalResponse = new();
        await responseStream.WriteAsync(incrementalResponse);

        Task clientCancellationTask = Task.Delay(Timeout.Infinite, context.CancellationToken);
        await Task.WhenAny(clientCancellationTask, grpcConnection.ProcessResponseWritesTask);
    }
}
