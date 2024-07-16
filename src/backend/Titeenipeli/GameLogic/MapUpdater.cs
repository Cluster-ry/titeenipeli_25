using Titeenipeli.Enums;
using Titeenipeli.GameLogic.MapCalculationDataTypes;
using Titeenipeli.Models;

namespace Titeenipeli.GameLogic;

using Coordinates = (int x, int y);
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
    /// <param name="pixelCoordinates">The coordinates of the new pixel</param>
    /// <param name="placingGuild">The guild placing the new pixel</param>
    public void PlacePixel(Map map, Coordinates pixelCoordinates, GuildName placingGuild)
    {
        map.Pixels[pixelCoordinates.y, pixelCoordinates.x]
            = new PixelModel { Owner = placingGuild, Type = PixelType.Normal };

        // TODO uncomment once _PlaceWithCache is implemented
        // TODO  - this should drop time complexity to O(log n) from O(n) if done right (MIT approved)
        // if (_cachedNodes is null)
        // {
        //     (nodes, justAddedNode) = _ConstructMapAdjacencyNodes(map, pixelCoordinates, placingGuild);
        // }
        // else
        // {
        //     (nodes, justAddedNode) = _PlaceWithCache(map, pixelCoordinates, placingGuild);
        // }
        var (nodes, justAddedNode) = _ConstructMapAdjacencyNodes(map, pixelCoordinates, placingGuild);

        // Construct a set of all nodes reachable from outside node (index 0)
        var nonHangingNodeIndexes = _GetNonSurroundedNodes(nodes, justAddedNode);

        foreach (var nodeIndex in nodes.Keys.Where(nodeIndex => !nonHangingNodeIndexes.Contains(nodeIndex)))
        {
            _FillNode(map, nodes[nodeIndex], placingGuild);
            nodes.Remove(nodeIndex);
        }

        _CutNodesWithoutSpawn(map, nodes);

        _cachedNodes = nodes;
    }

    private HashSet<int> _GetNonSurroundedNodes(AreaNodes nodes, int justAddedNode)
    {
        var nonSurroundedNodeIndexes = new HashSet<int>();
        _DfsWithBlockingNode(0, nodes, justAddedNode, nonSurroundedNodeIndexes);
        return nonSurroundedNodeIndexes;
    }

    private (AreaNodes, int) _PlaceWithCache(Map map, Coordinates pixelCoordinates, GuildName placingGuild)
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
    /// <param name="placingGuild">The guild that placed the last added pixel</param>
    /// <returns></returns>
    private (AreaNodes, int) _ConstructMapAdjacencyNodes(Map map, Coordinates pixelCoordinates,
        GuildName placingGuild)
    {
        var ySize = map.Pixels.GetUpperBound(0) + 1;
        var xSize = map.Pixels.GetUpperBound(1) + 1;
        var nodes = new AreaNodes();
        var nodeMap = new int[ySize, xSize];
        nodes[0] = _CreateOutsideNode(ySize, xSize);
        var nextNode = 1;
        for (var y = 1; y < ySize; y++)
        {
            for (var x = 1; x < xSize; x++)
            {
                var (leftNode, aboveNode) = TryMerge(placingGuild, nodeMap, y, x, nodes);

                var currentPixelGuild = map.Pixels[y, x].Owner;
                var isSpawnNode = map.Pixels[y, x].Type == PixelType.Spawn;

                if (nodes[leftNode].Guild == currentPixelGuild)
                {
                    AddPixelToNodeWithNeighbour(nodes, leftNode, (x, y), isSpawnNode, aboveNode, nodeMap);
                }
                else if (nodes[aboveNode].Guild == currentPixelGuild)
                {
                    AddPixelToNodeWithNeighbour(nodes, aboveNode, (x, y), isSpawnNode, leftNode, nodeMap);
                }
                else
                {
                    nodes[nextNode] = new Node
                    {
                        Pixels = { (x, y) },
                        Guild = currentPixelGuild,
                        Neighbours = { aboveNode, leftNode },
                        HasSpawn = isSpawnNode
                    };
                    nodes[leftNode].Neighbours.Add(nextNode);
                    nodes[aboveNode].Neighbours.Add(nextNode);
                    nodeMap[y, x] = nextNode;
                    nextNode++;
                }
            }
        }

        var justAddedNode = nodeMap[pixelCoordinates.x, pixelCoordinates.y];
        return (nodes, justAddedNode);
    }

    private static void AddPixelToNodeWithNeighbour(AreaNodes nodes, int destinationNode, Coordinates coordinates,
        bool isSpawnNode,
        int neighbourNode, int[,] nodeMap)
    {
        nodes[destinationNode].Pixels.Add(coordinates);
        nodes[destinationNode].HasSpawn = nodes[destinationNode].HasSpawn || isSpawnNode;
        nodes[destinationNode].Neighbours.Add(neighbourNode);
        nodes[neighbourNode].Neighbours.Add(destinationNode);
        nodeMap[coordinates.y, coordinates.x] = destinationNode;
    }

    private (int, int) TryMerge(GuildName mergingGuild, int[,] nodeMap, int y, int x, AreaNodes nodes)
    {
        var leftNode = nodeMap[y, x - 1];
        var aboveNode = nodeMap[y - 1, x];

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
            outsideNode.Pixels.Add((x, 0));
            outsideNode.Pixels.Add((x, ySize - 1));
        }

        for (var y = 0; y < xSize; y++)
        {
            outsideNode.Pixels.Add((0, y));
            outsideNode.Pixels.Add((xSize - 1, y));
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
            nodeMap[y, x] = smallerNodeIndex;
        }

        nodes[smallerNodeIndex].Pixels.UnionWith(nodes[largerNodeIndex].Pixels);
        nodes[smallerNodeIndex].Neighbours.UnionWith(nodes[largerNodeIndex].Neighbours);
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

    private void _FillNode(Map map, Node node, GuildName? newGuild)
    {
        foreach (var (x, y) in node.Pixels)
        {
            if (map.Pixels[y, x].Type == PixelType.Spawn)
            {
                continue;
            }

            map.Pixels[y, x].Owner = newGuild;
            map.Pixels[y, x].Type = PixelType.Normal;
        }
    }

    private void _CutNodesWithoutSpawn(Map map, AreaNodes nodes)
    {
        foreach (var node in nodes.Values.Where(node => node is { HasSpawn: false, Guild: not null }))
        {
            _FillNode(map, node, null);
        }
    }
}