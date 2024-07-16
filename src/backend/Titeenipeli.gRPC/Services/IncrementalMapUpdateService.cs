using Grpc.Core;
using GrpcGeneratedServices;
using Microsoft.Extensions.Logging;

namespace Titeenipeli.gRPC.Services;

public class IncrementalMapUpdateService(ILogger<IncrementalMapUpdateService> logger) : MapUpdate.MapUpdateBase
{
    private readonly ILogger<IncrementalMapUpdateService> _logger = logger;

    public override Task<IncrementalMapUpdateResponse> GetIncremental(IncrementalMapUpdateRequest request, ServerCallContext context)
    {
        _logger.LogInformation("IncrementalMapUpdate gRPC endpoint received: {request}", request);
        return Task.FromResult(new IncrementalMapUpdateResponse());
    }
}
