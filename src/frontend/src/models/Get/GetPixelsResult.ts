import { EncodedPixel } from "../Pixel";
import { PlayerCoordinates } from "../PlayerCoordinates";

export interface GetPixelsResult {
    playerSpawn: PlayerCoordinates;
    pixels: EncodedPixel[][];
}
