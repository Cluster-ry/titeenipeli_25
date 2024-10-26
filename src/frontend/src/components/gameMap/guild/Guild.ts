import { Pixel } from "../../../models/Pixel";
import Guild from "../../../models/enum/Guild";
import PixelType from "../../../models/enum/PixelType";
import {HslaColour} from "../../../models/HslaColour.ts";

const guildColorMapping: Record<Guild, HslaColour> = {
  [Guild.Tietokilta]: {hue: 0, saturation: 100, lightness: 42},
  [Guild.Algo]: {hue: 333, saturation: 84, lightness: 42},
  [Guild.Cluster]: {hue: 280, saturation: 100, lightness: 50},
  [Guild.OulunTietoteekkarit]: {hue: 265, saturation: 100, lightness: 46},
  [Guild.TietoTeekkarikilta]: {hue: 231, saturation: 99, lightness: 59},
  [Guild.Digit]: {hue: 0, saturation: 100, lightness: 50},
  [Guild.Datateknologerna]: {hue: 203, saturation: 100, lightness: 46},
  [Guild.Sosa]: {hue: 188, saturation: 100, lightness: 42}
};

export function pixelColor(pixel?: Pixel): HslaColour {
  const black = {hue: 0, saturation: 0, lightness: 0}

  if (!pixel) {
    return black;
  }

  if (pixel.type === PixelType.MapBorder) {
    return {hue: 44, saturation: 98, lightness: 50};
  }

  if (pixel.owner === undefined) {
    return black;
  }

  return guildColorMapping[pixel.owner as Guild] ?? black;
}
