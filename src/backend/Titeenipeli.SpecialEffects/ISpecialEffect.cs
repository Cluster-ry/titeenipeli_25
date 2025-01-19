using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;

namespace Titeenipeli.SpecialEffects;

public interface ISpecialEffect
{
    public string Description { get; }
    public bool Directed { get; }

    public List<Coordinate> HandleSpecialEffect(Coordinate location, Direction direction);
}