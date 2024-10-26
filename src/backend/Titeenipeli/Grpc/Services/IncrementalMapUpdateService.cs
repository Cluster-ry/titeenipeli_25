using Grpc.Core;
using GrpcGeneratedServices;
using Titeenipeli.Extensions;
using Titeenipeli.Grpc.Common;
using Titeenipeli.Services;
using Titeenipeli.Services.RepositoryServices.Interfaces;

namespace Titeenipeli.Grpc.Services;

public class IncrementalMapUpdateService(
    JwtService jwtService,
    IUserRepositoryService userRepositoryService,
    IIncrementalMapUpdateCoreService incrementalMapUpdateCoreService
) : MapUpdate.MapUpdateBase
{
    public override async Task GetIncremental(IncrementalMapUpdateRequest request,
                                              IServerStreamWriter<IncrementalMapUpdateResponse> responseStream,
                                              ServerCallContext context)
    {
        var httpContext = context.GetHttpContext();

        var user = httpContext.GetUser(jwtService, userRepositoryService);

        GrpcConnection<IncrementalMapUpdateResponse> grpcConnection =
            new(user, responseStream, incrementalMapUpdateCoreService.RemoveGrpcConnection);

        incrementalMapUpdateCoreService.AddGrpcConnection(grpcConnection);

        IncrementalMapUpdateResponse incrementalResponse = new();
        await responseStream.WriteAsync(incrementalResponse);

        var clientCancellationTask = Task.Delay(Timeout.Infinite, context.CancellationToken);
        await Task.WhenAny(clientCancellationTask, grpcConnection.ProcessResponseWritesTask);
    }
}