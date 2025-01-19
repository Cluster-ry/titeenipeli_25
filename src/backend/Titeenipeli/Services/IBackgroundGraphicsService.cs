using Titeenipeli.Common.Models;

namespace Titeenipeli.Services;

public interface IBackgroundGraphicsService
{
    public byte[]? GetBackgroundGraphic(Coordinate coordinate);
}