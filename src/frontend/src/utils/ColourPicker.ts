import { ColorOverlayFilter } from "pixi-filters";
import { Color } from "pixi.js";

interface IColourPicker {
    colours: Map<string, ColorOverlayFilter>;
    getColourOverlay: (hue: number, saturation: number, lightness: number, alpha: number) => ColorOverlayFilter;
};

class ColourPicker implements IColourPicker {
    constructor(){}
    colours = new Map();
    getColourOverlay = (hue: number = 0, saturation: number = 0, lightness: number = 0, alpha: number = 0.5) => {
        const colourId: string = `${hue}-${saturation}-${lightness}-${alpha}`;
        if (this.colours.has(colourId)) return this.colours.get(colourId);
        const colourValue: Color = new Color(`hsl(${hue}, ${saturation}%, ${lightness}%)`);
        const colour: ColorOverlayFilter = new ColorOverlayFilter([colourValue.red, colourValue.green, colourValue.blue], alpha);
        this.colours.set(colourId, colour);
        return colour;
    };
}

const colourPicker = new ColourPicker();

export default colourPicker;