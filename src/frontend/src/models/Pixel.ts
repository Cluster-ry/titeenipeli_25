import PixelType from "./enum/PixelType";
import { PixelOwner } from "../generated/grpc/components/enums/pixelOwner.ts";

export interface Pixel {
    type: PixelType;
    guild: PixelOwner | undefined;
    owner: number | undefined;
}
