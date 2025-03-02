using Titeenipeli.Common.Models;

namespace Titeenipeli.SpecialEffects;

public class GlitchEffect : BaseSpecialEffect
{
    public override string Description { get; } = "";   //TODO
                                                //jotain glitchi tekstiä
    protected override byte[,] Template { get; } = GetGlitchTemplate();

    private static byte[,] GetGlitchTemplate()
    {
        byte[,] bytes = new byte[5,5];
        Random rnd = new Random();
        
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                bytes[i, j] = (byte)rnd.Next(0, 2);
            }
        }
        return bytes;
    }

    protected override Coordinate Origin { get; } = new(0, 0);
}