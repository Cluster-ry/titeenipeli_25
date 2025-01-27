import { Pixel } from "../../models/Pixel.ts";
import Guild from "../../models/enum/Guild.ts";
import PixelType from "../../models/enum/PixelType.ts";
import { HslaColour } from "../../models/HslaColour.ts";
import { User } from "../../models/User.ts";

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

const lightnessChange = 20
const saturationChange = 40

export function pixelColor(pixel: Pixel | null, user: User | null): HslaColour | undefined {
    let color = pixelBaseColor(pixel);
    if (color === undefined) {
        return undefined;
    }

    color = pixelOwnerModifier(pixel, user, color);
    color = spawnModifier(pixel, color);
    return color;
}

const pixelBaseColor = (pixel?: Pixel | null) => {
    if (!pixel) {
        return undefined;
    }

    if (pixel.type === PixelType.MapBorder) {
        return mapBorderColor;
    }

    if (pixel.guild === undefined) {
        return undefined;
    }

    return guildColorMapping[pixel.guild as unknown as Guild] ?? undefined;
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
        saturation: color.saturation - saturationChange,
        lightness: color.lightness,
    };

    return alteredColor;
};

const spawnModifier = (pixel: Pixel | null, color: HslaColour): HslaColour => {
    if (pixel?.type !== PixelType.Spawn) {
        return color;
    }

    const alteredColor: HslaColour = {
        hue: color.hue,
        saturation: color.saturation,
        lightness: color.lightness - lightnessChange,
    };

    return alteredColor;
};