using System.Collections.Concurrent;
using Titeenipeli.Grpc.Common;

namespace Titeenipeli.Grpc.Services;

public class GrpcService<TResponseStream> : IGrpcService<TResponseStream> where TResponseStream : new()
{
    protected readonly ConcurrentDictionary<int, ConcurrentDictionary<int, IGrpcConnection<
            TResponseStream>>>
        Connections = new();

    public void AddGrpcConnection(
        IGrpcConnection<TResponseStream> connection)
    {
        ConcurrentDictionary<int, IGrpcConnection<TResponseStream>> userConnections = Connections.GetOrAdd(
            connection.User.Id, new ConcurrentDictionary<int, IGrpcConnection<TResponseStream>>());

        userConnections.TryAdd(connection.Id, connection);
    }

    public void RemoveGrpcConnection(
        IGrpcConnection<TResponseStream> connection)
    {
        bool retrievalSucceeded = Connections.TryGetValue(connection.User.Id,
            out ConcurrentDictionary<int, IGrpcConnection<TResponseStream>>? dictionaryOutput);

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