using Titeenipeli.Enums;
using Titeenipeli.GameLogic.MapCalculationDataTypes;
using Titeenipeli.Models;

namespace Titeenipeli.GameLogic;

// This class is meant to encapsulate all the complexity of map updating. We prefer raw data types and arrays to
// object based approaches to gain more speed. The sharp cut point for low-level code vs. high level code is the
// PlacePixel interface - no raw data type etc. complexity should bleed above it.

using GameMap = GuildEnum?[,];
using Coordinates = (int, int);
using AreaNodes = Dictionary<int, Node>;

public class MapUpdater
{
    // We can move this cache to Redis when Redis is up to eliminate state - although this is faster
    private AreaNodes? _cachedNodes;


    public MapModel PlacePixel(MapModel map, Coordinates pixelCoordinates, GuildEnum placingGuild)
    {
        AreaNodes nodes;
        int justAddedNode;
        map.Pixels[pixelCoordinates.Item2, pixelCoordinates.Item1] = new PixelModel
            { Owner = placingGuild, Type = PixelTypeEnum.Normal };
        if (_cachedNodes is null)
        {
            (nodes, justAddedNode) = _ConstructMapAdjacencyNodes(map, pixelCoordinates, placingGuild);
        }
        else
        {
            (nodes, justAddedNode) = _PlaceWithCache(map, pixelCoordinates, placingGuild);
        }

        var nonHangingNodeIndexes = new HashSet<int>();
        _TraverseNodeTree(0, nodes, justAddedNode, nonHangingNodeIndexes);

        foreach (var nodeIndex in nodes.Keys.Where(nodeIndex => !nonHangingNodeIndexes.Contains(nodeIndex)))
        {
            _FillNode(map, nodes[nodeIndex], placingGuild);
            nodes.Remove(nodeIndex);
        }

        _CutNodesWithoutSpawn(map, nodes);

        _cachedNodes = nodes;
        return map;
    }

    private (AreaNodes, int) _PlaceWithCache(MapModel map, Coordinates pixelCoordinates, GuildEnum placingGuild)
    {
        throw new NotImplementedException();
    }

    private (AreaNodes, int) _ConstructMapAdjacencyNodes(MapModel map, Coordinates pixelCoordinates, GuildEnum placingGuild)
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
                var leftNode = nodeMap[y, x - 1];
                var aboveNode = nodeMap[y - 1, x];
                var currentPixelGuild = map.Pixels[y, x].Owner;
                var isSpawnNode = map.Pixels[y, x].Type == PixelTypeEnum.Spawn;
                if (leftNode != aboveNode) {
                    var leftNodeGuild = nodes[leftNode].guild;
                    var aboveNodeGuild = nodes[aboveNode].guild;
                    if (leftNodeGuild == aboveNodeGuild)
                    {
                        leftNode = _MergeNodes(leftNode, aboveNode, nodes, nodeMap);
                        aboveNode = leftNode;
                    }
                }

                if (nodes[leftNode].guild == currentPixelGuild)
                {
                    nodes[leftNode].pixels.Add((x, y));
                    nodes[leftNode].hasSpawn = nodes[leftNode].hasSpawn || isSpawnNode;
                    nodes[leftNode].neighbours.Add(aboveNode);
                    nodes[aboveNode].neighbours.Add(leftNode);
                    nodeMap[y, x] = leftNode;
                }
                else if (nodes[aboveNode].guild == currentPixelGuild)
                {
                    nodes[aboveNode].pixels.Add((x, y));
                    nodes[aboveNode].hasSpawn = nodes[aboveNode].hasSpawn || isSpawnNode;
                    nodes[aboveNode].neighbours.Add(leftNode);
                    nodes[leftNode].neighbours.Add(aboveNode);
                }
                else
                {
                    nodes[nextNode] = new Node
                    {
                        pixels = { (x, y) },
                        guild = currentPixelGuild,
                        neighbours = { aboveNode, leftNode },
                        hasSpawn = isSpawnNode
                    };
                    nodes[leftNode].neighbours.Add(nextNode);
                    nodes[aboveNode].neighbours.Add(aboveNode);
                    nodeMap[y, x] = nextNode;
                    nextNode++;
                }
            }
        }

        var justAddedNode = nodeMap[pixelCoordinates.Item1, pixelCoordinates.Item2];
        return (nodes, justAddedNode);
    }

    private Node _CreateOutsideNode(int ySize, int xSize)
    {
        var outsideNode = new Node{guild = null};
        for (var x = 0; x < xSize; x++)
        {
            outsideNode.pixels.Add((x, 0));
            outsideNode.pixels.Add((x, ySize - 1));
        }
        
        for (var y = 0; y < xSize; y++)
        {
            outsideNode.pixels.Add((0, y));
            outsideNode.pixels.Add((xSize - 1, y));
        }

        return outsideNode;
    }

    private int _MergeNodes(int leftNodeIndex, int aboveNodeIndex, AreaNodes nodes, int[,] nodeMap)
    {
        var smallerNodeIndex = Math.Min(leftNodeIndex, aboveNodeIndex);
        var largerNodeIndex = Math.Max(leftNodeIndex, aboveNodeIndex);
        nodes[largerNodeIndex].neighbours.Remove(largerNodeIndex);
        foreach (var neighbourNodeIndex in nodes[largerNodeIndex].neighbours)
        {
            nodes[neighbourNodeIndex]?.neighbours.Remove(largerNodeIndex);
            nodes[neighbourNodeIndex]?.neighbours.Add(smallerNodeIndex);
        }

        foreach (var (x, y) in nodes[largerNodeIndex].pixels)
        {
            nodeMap[y, x] = smallerNodeIndex;
        }
        
        nodes[smallerNodeIndex].pixels.UnionWith(nodes[largerNodeIndex].pixels);
        nodes[smallerNodeIndex].neighbours.UnionWith(nodes[largerNodeIndex].neighbours);
        nodes[largerNodeIndex] = null;
        return smallerNodeIndex;
    }

    private void _TraverseNodeTree(int currentNode, AreaNodes nodes, int justAddedNode, HashSet<int> visitedNodes)
    {
        visitedNodes.Add(currentNode);
        if (currentNode == justAddedNode)
        {
            return;
        }

        foreach (var neighbour in nodes[currentNode].neighbours)
        {
            _TraverseNodeTree(neighbour, nodes, justAddedNode, visitedNodes);
        }
    }

    private void _FillNode(MapModel map, Node node, GuildEnum? newGuild)
    {
        foreach (var (x, y) in node.pixels)
        {
            if (map.Pixels[y, x].Type == PixelTypeEnum.Spawn)
            {
                continue;
            }
            map.Pixels[y, x].Owner = newGuild;
            map.Pixels[y, x].Type = PixelTypeEnum.Normal;
        }
    }

    private void _CutNodesWithoutSpawn(MapModel map, AreaNodes nodes)
    {
        foreach (var node in nodes.Values.Where(node => !node.hasSpawn))
        {
            _FillNode(map, node, null);
        }
    }
}