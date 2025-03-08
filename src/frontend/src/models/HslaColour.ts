import PixelType from "./enum/PixelType";
import { Pixel } from "./Pixel";
import { User } from "./User";

const lightnessChange = 20;
const saturationChange = 40;

export interface IHslaColour {
    hue: number;
    saturation: number;
    lightness: number;
    alpha: number;
    getColourWithModifiers: (pixel?: Pixel | null, user?: User | null) => IHslaColour;
}

export class HslaColour implements IHslaColour { 
    constructor(hue: number, saturation: number, lightness: number, alpha: number = 0.5){
        this.hue = hue;
        this.saturation = saturation;
        this.lightness = lightness;
        this.alpha = alpha;
    }
    hue: number;
    saturation: number;
    lightness: number;
    alpha: number;
    getColourWithModifiers(pixel: Pixel | null = null, user: User | null = null) {
        if (pixel == null && user == null) return this;
        const result: HslaColour = new HslaColour(this.hue, this.saturation, this.lightness, this.alpha);
        return result.saturatePixelOwner(pixel, user).lightenSpawn(pixel);

    };
    saturatePixelOwner(pixel: Pixel | null, user: User | null){
        if (pixel?.owner === user?.id && pixel?.guild === user?.guild) {
            this.saturation = this.saturation - saturationChange;
        }
        return this;
    }
    lightenSpawn(pixel: Pixel | null){
        if (pixel?.type === PixelType.Spawn) {
            this.lightness -= lightnessChange;;
        }
        return this;
    }
}
