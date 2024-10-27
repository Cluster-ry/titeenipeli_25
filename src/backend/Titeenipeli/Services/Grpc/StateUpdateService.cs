using Grpc.Core;
using GrpcGeneratedServices;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Extensions;
using Titeenipeli.Grpc.Common;
using Titeenipeli.Services;
using Titeenipeli.Services.Grpc;

namespace Titeenipeli.Grpc.Services;

public class StateUpdateService(
    JwtService jwtService,
    IUserRepositoryService userRepositoryService,
    IIncrementalMapUpdateCoreService incrementalMapUpdateCoreService,
    IMiscGameStateUpdateCoreService miscGameStateUpdateCoreService
) : StateUpdate.StateUpdateBase
{
    public override async Task GetIncrementalMapUpdate(IncrementalMapUpdateRequest request,
                                              IServerStreamWriter<IncrementalMapUpdateResponse> responseStream,
                                              ServerCallContext context)
    {
        var user = GetUserFromHttpContext(context);

        GrpcConnection<IncrementalMapUpdateResponse> grpcConnection =
            new(user, responseStream, incrementalMapUpdateCoreService.RemoveGrpcConnection);

        incrementalMapUpdateCoreService.AddGrpcConnection(grpcConnection);

        IncrementalMapUpdateResponse incrementalResponse = new();
        await responseStream.WriteAsync(incrementalResponse);

        var clientCancellationTask = Task.Delay(Timeout.Infinite, context.CancellationToken);
        await Task.WhenAny(clientCancellationTask, grpcConnection.ProcessResponseWritesTask);
    }

    public override async Task GetMiscGameStateUpdate(MiscStateUpdateRequest request,
                                              IServerStreamWriter<MiscStateUpdateResponse> responseStream,
                                              ServerCallContext context)
    {
        var user = GetUserFromHttpContext(context);

        GrpcConnection<MiscStateUpdateResponse> grpcConnection =
            new(user, responseStream, miscGameStateUpdateCoreService.RemoveGrpcConnection);

        miscGameStateUpdateCoreService.AddGrpcConnection(grpcConnection);

        MiscStateUpdateResponse incrementalResponse = new();
        await responseStream.WriteAsync(incrementalResponse);

        var clientCancellationTask = Task.Delay(Timeout.Infinite, context.CancellationToken);
        await Task.WhenAny(clientCancellationTask, grpcConnection.ProcessResponseWritesTask);
    }

    private User GetUserFromHttpContext(ServerCallContext context)
    {
        var httpContext = context.GetHttpContext();
        var jwtClaim = httpContext.GetUser(jwtService, userRepositoryService);
        if (jwtClaim == null)
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied,
                "Missing or invalid authentication cookie."));
        }

        var user = userRepositoryService.GetById(jwtClaim.Id);
        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, "User couldn't be found."));
        }

        return user;
    }
}