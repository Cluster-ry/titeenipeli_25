import { Pixel } from "../../../models/Pixel";
import Guild from "../../../models/enum/Guild";
import PixelType from "../../../models/enum/PixelType";
import { HslaColour } from "../../../models/HslaColour.ts";
import { User } from "../../../models/User.ts";

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
const mapBorderColor = { hue: 44, saturation: 98, lightness: 50 };
const black = { hue: 0, saturation: 0, lightness: 0 };

export function pixelColor(pixel: Pixel | null, user: User | null): HslaColour {
    let color = pixelBaseColor(pixel);
    color = pixelOwnerModifier(pixel, user, color);
    return color;
}

const pixelBaseColor = (pixel?: Pixel | null) => {
    if (!pixel) {
        return black;
    }

    if (pixel.type === PixelType.MapBorder) {
        return mapBorderColor;
    }

    if (pixel.guild === undefined) {
        return black;
    }

    return guildColorMapping[pixel.guild as unknown as Guild] ?? black;
};

/**
 * A pixel owned by the current user will have its saturation lowered
 */
const pixelOwnerModifier = (pixel: Pixel | null, user: User | null, color: HslaColour): HslaColour => {
    if (user === null || pixel === null) {
        return color;
    }

    if (pixel.owner !== user.id || pixel.guild !== user.guild) {
        return color;
    }

    const alteredColor: HslaColour = {
        hue: color.hue,
        saturation: color.saturation - 20,
        lightness: color.lightness,
    };

    return alteredColor;
};
