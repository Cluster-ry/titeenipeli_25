using System.Collections.Concurrent;
using GrpcGeneratedServices;
using Titeenipeli.Grpc.Common;

namespace Titeenipeli.Grpc.Services;

public class GrpcService : IGrpcService
{
    protected readonly ConcurrentDictionary<int, ConcurrentDictionary<int, IGrpcConnection<
            IncrementalMapUpdateResponse>>>
        Connections = new();

    public void AddGrpcConnection(
        IGrpcConnection<IncrementalMapUpdateResponse> connection)
    {
        ConcurrentDictionary<int, IGrpcConnection<IncrementalMapUpdateResponse>> userConnections = Connections.GetOrAdd(
            connection.User.Id, new ConcurrentDictionary<int, IGrpcConnection<IncrementalMapUpdateResponse>>());

        userConnections.TryAdd(connection.Id, connection);
    }

    public void RemoveGrpcConnection(
        IGrpcConnection<IncrementalMapUpdateResponse> connection)
    {
        bool retrievalSucceeded = Connections.TryGetValue(connection.User.Id,
            out ConcurrentDictionary<int, IGrpcConnection<IncrementalMapUpdateResponse>>? dictionaryOutput);

        if (!retrievalSucceeded || dictionaryOutput == null)
        {
            return;
        }

        connection.Dispose();
        dictionaryOutput.TryRemove(connection.Id, out _);

        if (dictionaryOutput.IsEmpty)
        {
            Connections.TryRemove(connection.User.Id, out _);
        }
    }
}