import PixelType from "./enum/PixelType";
import { PixelGuild } from "../generated/grpc/components/enums/pixelGuild.ts";

export interface Pixel {
    type: PixelType;
    guild: PixelGuild | undefined;
    owner: number | undefined;
    backgroundGraphic?: Uint8Array;
    highlight?: boolean;
}

export interface EncodedPixel {
    type: PixelType;
    guild: PixelGuild | undefined;
    owner: number | undefined;
    backgroundGraphic?: string;
}
