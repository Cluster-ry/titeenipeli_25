using Titeenipeli.Common.Models;

namespace Titeenipeli.SpecialEffects;

public interface ISpecialEffect
{
    public byte[,] Template { get; }
    public Coordinate Origin { get; }

    public List<Coordinate> HandleSpecialEffect(Coordinate location);
}