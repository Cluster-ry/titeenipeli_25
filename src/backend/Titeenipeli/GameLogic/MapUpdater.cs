using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;
using Titeenipeli.GameLogic.MapCalculationDataTypes;

namespace Titeenipeli.GameLogic;

using AreaNodes = Dictionary<int, Node>;

public class MapUpdater
{
    // We can move this cache to Redis when Redis is up to eliminate state - although this is faster
    private AreaNodes? _cachedNodes;

    // Note - this class encapsulates the most complex part of game logic - thus I made a decision to overcomment here
    // including adding doc comments. This should enable others to modify the file should I be unavailable -Eddie

    /// <summary>
    /// Place one pixel into the given map and calculate the resulting fill and cut operations
    /// </summary>
    /// <param name="map">The game map - should include border pixels. Mutated according to the update event.</param>
    /// <param name="pixelCoordinate">The coordinates of the new pixel</param>
    /// <param name="placingUser">The user placing the new pixel</param>
    /// <param name="type">The type of the pixel to place, defaults to PixelType.Normal</param>
    public List<MapChange> PlacePixel(PixelWithType[,] map, Coordinate pixelCoordinate, User placingUser, PixelType type = PixelType.Normal)
    {
        List<MapChange> allChangedPixels = [];
        User? oldOwner = map[pixelCoordinate.X, pixelCoordinate.Y].Owner;
        allChangedPixels.Add(new() { Coordinate = pixelCoordinate - new Coordinate(1, 1), OldOwner = oldOwner, NewOwner = placingUser });
        map[pixelCoordinate.X, pixelCoordinate.Y]
            = new PixelWithType { Location = pixelCoordinate - new Coordinate(1, 1), Type = type, Owner = placingUser };

        return UpdateMapAfterPixelChanges(allChangedPixels, map, pixelCoordinate, placingUser);
    }

    /// <summary>
    /// Place multiple pixels into the given map and calculate the resulting fill and cut operations
    /// </summary>
    /// <param name="map">The game map - should include border pixels. Mutated according to the update event.</param>
    /// <param name="pixelCoordinates">The coordinates of the new pixels</param>
    /// <param name="placingUser">The user placing the new pixels</param>
    public List<MapChange> PlacePixels(PixelWithType[,] map, Coordinate[] pixelCoordinates, User placingUser)
    {
        List<MapChange> allChangedPixels = [];
        if (pixelCoordinates.Length == 0)
        {
            return [];
        }

        var latestCoordinate = pixelCoordinates[0];
        foreach (var pixelCoordinate in pixelCoordinates)
        {
            var xSize = map.GetUpperBound(0) + 1;
            var ySize = map.GetUpperBound(1) + 1;
            // Normally you would see strict greater/less than, but due to border behaviour we can do just a minor
            // optimization here with greater/less than or equal
            var coordinateIsOutOfBounds =
                pixelCoordinate.Y <= 0 || pixelCoordinate.Y >= ySize ||
                pixelCoordinate.X <= 0 || pixelCoordinate.X >= xSize;

            if (coordinateIsOutOfBounds)
            {
                continue;
            }

            // Due to previous out of bound behaviour MapBorder should never be encountered here but better safe than
            // sorry. Overriding a border would be rather bad after all...
            var coordinateIsReadonly =
                map[pixelCoordinate.X, pixelCoordinate.Y].Type is PixelType.Spawn or PixelType.MapBorder;

            if (coordinateIsReadonly)
            {
                continue;
            }

            var oldOwner = map[pixelCoordinate.X, pixelCoordinate.Y].Owner;
            allChangedPixels.Add(new MapChange { Coordinate = pixelCoordinate - new Coordinate(1, 1), OldOwner = oldOwner, NewOwner = placingUser });
            map[pixelCoordinate.X, pixelCoordinate.Y]
                = new PixelWithType { Location = pixelCoordinate - new Coordinate(1, 1), Type = PixelType.Normal, Owner = placingUser };
            latestCoordinate = pixelCoordinate;
        }

        return UpdateMapAfterPixelChanges(allChangedPixels, map, latestCoordinate, placingUser);
    }

    private List<MapChange> UpdateMapAfterPixelChanges(
        List<MapChange> allChangedPixels,
        PixelWithType[,] map,
        Coordinate pixelCoordinate,
        User placingUser)
    {
        // TODO uncomment once _PlaceWithCache is implemented
        // this should drop time complexity to O(log n) from O(n) if done right (MIT approved)
        //
        // Well shit we don't have time for this after code complexity jumped due to requirement changes
        // Now this algorithm is just plain worse than simple flood-fill with no fancy data structure. My bad - Eddie
        // if (_cachedNodes is null)
        // {
        //     (nodes, justAddedNode) = _ConstructMapAdjacencyNodes(map, pixelCoordinates, placingGuild);
        // }
        // else
        // {
        //     (nodes, justAddedNode) = _PlaceWithCache(map, pixelCoordinates, placingGuild);
        // }
        var (nodes, justAddedNode) = _ConstructMapAdjacencyNodes(map, pixelCoordinate);

        // Construct a set of all nodes reachable from outside node (index 0)
        var nonHangingNodeIndexes = _GetNonSurroundedNodes(nodes, justAddedNode);

        var changedPixels = _CutNodesWithoutSpawn(map, nodes);
        allChangedPixels = [.. allChangedPixels, .. changedPixels];

        // Fill is applied only for nodes owned by nobody.
        foreach (var nodeIndex in nodes.Keys.Where(
                     nodeIndex => !nonHangingNodeIndexes.Contains(nodeIndex) &&
                                  (nodes[nodeIndex].Guild == null || nodes[nodeIndex].Guild == GuildName.Nobody)))
        {
            var changedFillPixels = _FillNode(map, nodes[nodeIndex], placingUser);
            allChangedPixels = [.. allChangedPixels, .. changedFillPixels];
            nodes.Remove(nodeIndex);
        }

        _cachedNodes = nodes;

        return allChangedPixels;
    }

    private HashSet<int> _GetNonSurroundedNodes(AreaNodes nodes, int justAddedNode)
    {
        var nonSurroundedNodeIndexes = new HashSet<int>();
        _DfsWithBlockingNode(0, nodes, justAddedNode, nonSurroundedNodeIndexes);
        return nonSurroundedNodeIndexes;
    }

    private (AreaNodes, int) _PlaceWithCache(Map map, Coordinate pixelCoordinates, GuildName placingGuild)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// Builds a graph from all nodes (single-color areas) in the given map. Includes ownerless nodes (color = null).
    /// The outside of the map is considered an ownerless node and will always have an index of 0.
    /// <remarks>
    /// Also returns the index of the node that contains the last added pixel
    /// </remarks>
    /// </summary>
    /// <param name="map">The map state to construct a graph of</param>
    /// <param name="pixelCoordinates">The coordinates of the last added pixel</param>
    /// <returns></returns>
    private (AreaNodes, int) _ConstructMapAdjacencyNodes(PixelWithType[,] map, Coordinate pixelCoordinates)
    {
        var xSize = map.GetUpperBound(0) + 1;
        var ySize = map.GetUpperBound(1) + 1;
        var nodes = new AreaNodes();
        var nodeMap = new int[xSize, ySize];
        nodes[0] = _CreateOutsideNode(xSize, ySize);
        var nextNode = 1;
        for (var y = 1; y < ySize; y++)
        {
            for (var x = 1; x < xSize; x++)
            {
                var currentPixelGuild = map[x, y].Owner?.Guild?.Name;
                var (leftNode, aboveNode) = TryMerge(currentPixelGuild, nodeMap, y, x, nodes);

                var isSpawnNode = map[x, y].Type == PixelType.Spawn;

                if (nodes[leftNode].Guild == currentPixelGuild)
                {
                    AddPixelToNodeWithNeighbour(nodes, leftNode, new(x, y), isSpawnNode, aboveNode, nodeMap);
                }
                else if (nodes[aboveNode].Guild == currentPixelGuild)
                {
                    AddPixelToNodeWithNeighbour(nodes, aboveNode, new(x, y), isSpawnNode, leftNode, nodeMap);
                }
                else
                {
                    nodes[nextNode] = new Node
                    {
                        Pixels = { new(x, y) },
                        Guild = currentPixelGuild,
                        Neighbours = { aboveNode, leftNode },
                        HasSpawn = isSpawnNode
                    };
                    nodes[leftNode].Neighbours.Add(nextNode);
                    nodes[aboveNode].Neighbours.Add(nextNode);
                    nodeMap[x, y] = nextNode;
                    nextNode++;
                }
            }
        }

        var justAddedNode = nodeMap[pixelCoordinates.X, pixelCoordinates.Y];
        return (nodes, justAddedNode);
    }

    private static void AddPixelToNodeWithNeighbour(AreaNodes nodes, int destinationNode, Coordinate coordinates,
        bool isSpawnNode,
        int neighbourNode, int[,] nodeMap)
    {
        nodes[destinationNode].Pixels.Add(coordinates);
        nodes[destinationNode].HasSpawn = nodes[destinationNode].HasSpawn || isSpawnNode;
        nodes[destinationNode].Neighbours.Add(neighbourNode);
        nodes[neighbourNode].Neighbours.Add(destinationNode);
        nodeMap[coordinates.X, coordinates.Y] = destinationNode;
    }

    private (int, int) TryMerge(GuildName? mergingGuild, int[,] nodeMap, int y, int x, AreaNodes nodes)
    {
        var leftNode = nodeMap[x - 1, y];
        var aboveNode = nodeMap[x, y - 1];

        if (leftNode == aboveNode)
        {
            return (leftNode, aboveNode);
        }

        var leftNodeGuild = nodes[leftNode].Guild;
        var aboveNodeGuild = nodes[aboveNode].Guild;
        var nodesHaveDifferingOwners = leftNodeGuild != aboveNodeGuild || leftNodeGuild != mergingGuild;
        if (nodesHaveDifferingOwners)
        {
            return (leftNode, aboveNode);
        }

        leftNode = _MergeNodes(leftNode, aboveNode, nodes, nodeMap);
        aboveNode = leftNode;

        return (leftNode, aboveNode);
    }

    private Node _CreateOutsideNode(int ySize, int xSize)
    {
        var outsideNode = new Node { Guild = null };
        for (var x = 0; x < xSize; x++)
        {
            outsideNode.Pixels.Add(new(x, 0));
            outsideNode.Pixels.Add(new(x, ySize - 1));
        }

        for (var y = 0; y < xSize; y++)
        {
            outsideNode.Pixels.Add(new(0, y));
            outsideNode.Pixels.Add(new(xSize - 1, y));
        }

        return outsideNode;
    }

    private int _MergeNodes(int leftNodeIndex, int aboveNodeIndex, AreaNodes nodes, int[,] nodeMap)
    {
        var smallerNodeIndex = Math.Min(leftNodeIndex, aboveNodeIndex);
        var largerNodeIndex = Math.Max(leftNodeIndex, aboveNodeIndex);
        nodes[largerNodeIndex].Neighbours.Remove(largerNodeIndex);
        foreach (var neighbourNodeIndex in nodes[largerNodeIndex].Neighbours)
        {
            nodes[neighbourNodeIndex].Neighbours.Remove(largerNodeIndex);
            nodes[neighbourNodeIndex].Neighbours.Add(smallerNodeIndex);
        }

        foreach (var (x, y) in nodes[largerNodeIndex].Pixels)
        {
            nodeMap[x, y] = smallerNodeIndex;
        }

        nodes[smallerNodeIndex].Pixels.UnionWith(nodes[largerNodeIndex].Pixels);
        nodes[smallerNodeIndex].Neighbours.UnionWith(nodes[largerNodeIndex].Neighbours);
        nodes[smallerNodeIndex].HasSpawn = nodes[smallerNodeIndex].HasSpawn || nodes[largerNodeIndex].HasSpawn;
        nodes.Remove(largerNodeIndex);
        return smallerNodeIndex;
    }

    /// <summary>
    /// A recursive depth-first search for the node graph. Treats the given just added node as a dead end. This prevents
    /// the algorithm finding nodes surrounded only by it, meaning nodes that should be filled.
    /// </summary>
    /// <param name="currentNode">The current node in the recursive search algorithm</param>
    /// <param name="nodes">All nodes</param>
    /// <param name="blockingNode">The node containing the last added pixel - will block search</param>
    /// <param name="visitedNodes">Nodes visited so far by the search. Mutated as a part of the algorithm.</param>
    private void _DfsWithBlockingNode(
        int currentNode,
        AreaNodes nodes,
        int blockingNode,
        HashSet<int> visitedNodes)
    {
        // This is the recursion escape clause - for some reason ReSharper doesn't realize that
        // ReSharper disable once CanSimplifySetAddingWithSingleCall
        if (visitedNodes.Contains(currentNode))
        {
            return;
        }

        visitedNodes.Add(currentNode);
        if (currentNode == blockingNode)
        {
            return;
        }

        foreach (var neighbour in nodes[currentNode].Neighbours)
        {
            _DfsWithBlockingNode(neighbour, nodes, blockingNode, visitedNodes);
        }
    }

    private List<MapChange> _FillNode(PixelWithType[,] map, Node node, User? placingUser)
    {
        List<MapChange> changedPixels = [];
        foreach (var (x, y) in node.Pixels)
        {
            if (map[x, y].Type == PixelType.Spawn)
            {
                continue;
            }

            MapChange change = new() { Coordinate = new Coordinate(x - 1, y - 1), OldOwner = map[x, y].Owner, NewOwner = placingUser };
            changedPixels.Add(change);
            map[x, y].Owner = placingUser;
            map[x, y].Type = PixelType.Normal;
        }
        return changedPixels;
    }

    private List<MapChange> _CutNodesWithoutSpawn(PixelWithType[,] map, AreaNodes nodes)
    {
        List<MapChange> allChangedPixels = [];
        foreach (var node in nodes.Values.Where(node => node is { HasSpawn: false, Guild: not null }))
        {
            node.Guild = null;
            List<MapChange> changedPixels = _FillNode(map, node, null);
            allChangedPixels = [.. allChangedPixels, .. changedPixels];
        }
        return allChangedPixels;
    }
}