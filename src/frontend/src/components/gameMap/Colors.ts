import { Pixel } from "../../models/Pixel.ts";
import Guild from "../../models/enum/Guild.ts";
import PixelType from "../../models/enum/PixelType.ts";
import { HslaColour, IHslaColour } from "../../models/HslaColour.ts";
import { User } from "../../models/User.ts";

/**
 * The color mapping for each of the Software Engineering guilds
 *
 * All guilds have their own dedicated color, and it is shown
 * on the game map in the conquered pixels.
 */
const guildColorMapping: Record<Guild, HslaColour> = {
    [Guild.Nobody]: new HslaColour(0, 0, 0, 0),
    [Guild.Tietokilta]: new HslaColour(23, 100, 50),
    [Guild.Algo]: new HslaColour(75, 100, 50),
    [Guild.Cluster]: new HslaColour(0, 100, 50),
    [Guild.OulunTietoteekkarit]: new HslaColour(94, 100, 50),
    [Guild.TietoTeekkarikilta]: new HslaColour(155, 100, 50),
    [Guild.Digit]: new HslaColour(178, 100, 50),
    [Guild.Datateknologerna]: new HslaColour(212, 100, 50),
    [Guild.Sosa]: new HslaColour(240, 100, 50),
    [Guild.Date]: new HslaColour(280, 100, 50),
    [Guild.Tutti]: new HslaColour(312, 100, 50),
};
const mapBorderColor = new HslaColour(44, 98, 50, 1);

export function pixelColor(pixel: Pixel | null = null, user: User | null = null): IHslaColour {
    const color: IHslaColour = pixelBaseColor(pixel);
    return color.getColourWithModifiers(pixel, user);
}

const pixelBaseColor = (pixel?: Pixel | null): IHslaColour => {
    if (pixel?.type === PixelType.MapBorder) {
        return mapBorderColor;
    }
    return guildColorMapping[pixel?.guild ?? Guild.Nobody] ?? guildColorMapping[Guild.Nobody];
}
