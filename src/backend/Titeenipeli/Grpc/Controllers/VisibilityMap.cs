using Titeenipeli.Models;

namespace Titeenipeli.Grpc.Controllers;

public class VisibilityMap(int mapWidth, int mapHeight, int fogOfWarDistance)
{
    private int _fogOfWarDistance = fogOfWarDistance;
    private bool[,] _visibilityMap = new bool[mapWidth + 2 * fogOfWarDistance, mapHeight + 2 * fogOfWarDistance];

    public void SetVisible(Coordinate coordinate)
    {
        _visibilityMap[coordinate.X + _fogOfWarDistance, coordinate.Y + _fogOfWarDistance] = true;
    }

    public bool GetVisibility(Coordinate coordinate)
    {
        return _visibilityMap[coordinate.X + _fogOfWarDistance, coordinate.Y + _fogOfWarDistance];
    }
}