import { Pixel } from "../../../models/Pixel";
import Guild from "../../../models/enum/Guild";
import PixelType from "../../../models/enum/PixelType";

const guildColorMapping: Record<Guild, number> = {
  [Guild.Tietokilta]: 0xd50000,
  [Guild.Algo]: 0xc51162,
  [Guild.Cluster]: 0xaa00ff,
  [Guild.OulunTietoteekkarit]: 0x6200ea,
  [Guild.TietoTeekkarikilta]: 0x304ffe,
  [Guild.Digit]: 0x2962ff,
  [Guild.Datateknologerna]: 0x0091ea,
  [Guild.Sosa]: 0x00b8d4,
};

export function pixelColor(pixel: Pixel | undefined): number {
  if (!pixel) {
    return 0x000000;
  }

  if (pixel.type === PixelType.MapBorder) {
    return 0xfcba03;
  }

  if (pixel.owner === undefined) {
    return 0x000000;
  }

  return guildColorMapping[pixel.owner as Guild] ?? 0x000000;
}
