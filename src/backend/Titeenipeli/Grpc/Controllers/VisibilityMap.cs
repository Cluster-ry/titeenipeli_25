using Titeenipeli.Models;

namespace Titeenipeli.Grpc.Controllers;

public class VisibilityMap(int mapWidth, int mapHeight)
{
    private int _mapWidth = mapWidth;
    private int _mapHeight = mapHeight;
    private bool[,] _visibilityMap = new bool[mapWidth, mapHeight];

    public void SetVisible(Coordinate coordinate)
    {
        if (IsInsideMap(coordinate))
        {
            _visibilityMap[coordinate.X, coordinate.Y] = true;
        }
    }

    public bool GetVisibility(Coordinate coordinate)
    {
        if (IsInsideMap(coordinate))
        {
            return _visibilityMap[coordinate.X, coordinate.Y];
        }
        else
        {
            return false;
        }
    }

    private bool IsInsideMap(Coordinate coordinate)
    {
        return coordinate.X >= 0 && coordinate.X < _mapWidth && coordinate.Y >= 0 && coordinate.Y < _mapHeight;
    }
}