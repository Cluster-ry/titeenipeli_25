import { Pixel } from "../../../models/Pixel";
import Guild from "../../../models/enum/Guild";
import PixelType from "../../../models/enum/PixelType";
import { HslaColour } from "../../../models/HslaColour.ts";

/**
 * The color mapping for each of the Software Engineering guilds
 *
 * All guilds have their own dedicated color, and it is shown
 * on the game map in the conquered pixels.
 */
const guildColorMapping: Record<Guild, HslaColour> = {
    [Guild.Nobody]: { hue: 0, saturation: 0, lightness: 0 },
    [Guild.Tietokilta]: { hue: 23, saturation: 100, lightness: 50 },
    [Guild.Algo]: { hue: 75, saturation: 100, lightness: 50 },
    [Guild.Cluster]: { hue: 0, saturation: 100, lightness: 50 },
    [Guild.OulunTietoteekkarit]: { hue: 94, saturation: 100, lightness: 50 },
    [Guild.TietoTeekkarikilta]: { hue: 155, saturation: 100, lightness: 50 },
    [Guild.Digit]: { hue: 178, saturation: 100, lightness: 50 },
    [Guild.Datateknologerna]: { hue: 212, saturation: 100, lightness: 50 },
    [Guild.Sosa]: { hue: 240, saturation: 100, lightness: 50 },
    [Guild.Date]: { hue: 280, saturation: 100, lightness: 50 },
    [Guild.Tutti]: { hue: 312, saturation: 100, lightness: 50 },
};

export function pixelColor(pixel?: Pixel | null): HslaColour {
    const black = { hue: 0, saturation: 0, lightness: 0 };

    if (!pixel) {
        return black;
    }

    if (pixel.type === PixelType.MapBorder) {
        return { hue: 44, saturation: 98, lightness: 50 };
    }

    if (pixel.guild === undefined) {
        return black;
    }

    return guildColorMapping[pixel.guild as unknown as Guild] ?? black;
}
