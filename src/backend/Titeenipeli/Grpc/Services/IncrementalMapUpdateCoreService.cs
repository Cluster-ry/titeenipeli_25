using System.Collections.Concurrent;
using GrpcGeneratedServices;
using Titeenipeli.Grpc.Common;

namespace Titeenipeli.Grpc.Services;

public class IncrementalMapUpdateCoreService(ILogger<IncrementalMapUpdateService> logger) : IIncrementalMapUpdateCoreService
{
    private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, GrpcConnection<IncrementalMapUpdateResponse>>> Connections = new();

    private readonly ILogger<IncrementalMapUpdateService> _logger = logger;

    public void AddGrpcConnection(GrpcConnection<IncrementalMapUpdateResponse> connection)
    {
        ConcurrentDictionary<int, GrpcConnection<IncrementalMapUpdateResponse>> userConnections = Connections.GetOrAdd(connection.User.Id, new ConcurrentDictionary<int, GrpcConnection<IncrementalMapUpdateResponse>>());
        userConnections.TryAdd(connection.Id, connection);
    }

    public void RemoveGrpcConnection(GrpcConnection<IncrementalMapUpdateResponse> connection)
    {
        ConcurrentDictionary<int, GrpcConnection<IncrementalMapUpdateResponse>>? dictionaryOutput;
        var retrievalSucceeded = Connections.TryGetValue(connection.User.Id, out dictionaryOutput);
        if (retrievalSucceeded)
        {
            dictionaryOutput?.TryRemove(connection.Id, out _);
        }
    }
}
