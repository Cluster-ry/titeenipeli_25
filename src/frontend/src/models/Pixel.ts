import PixelType from "./enum/PixelType";

export interface Pixel {
  type: PixelType;
  owner: number | undefined;
  ownPixel: boolean;
}
