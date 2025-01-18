using Titeenipeli.Common.Enums;

namespace Titeenipeli.Inputs;

public sealed class PowerInput
{
    public required int Id { get; init; }
    public required PostPixelsInput Location { get; init; }
    public required Direction Direction { get; init; }

}