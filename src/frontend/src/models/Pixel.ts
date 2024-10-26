import PixelType from "./enum/PixelType";

export interface Pixel {
  type: PixelType;
  guild: number | undefined;
  owner: number | undefined;
  ownPixel: boolean;
}
