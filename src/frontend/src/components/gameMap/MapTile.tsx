import { PixiComponent } from "@pixi/react";
import { FederatedPointerEvent, Sprite, Texture } from "pixi.js";
import MapTileProps from "../../models/MapTileProps";
import { ColorOverlayFilter, GlowFilter } from "pixi-filters";
import colourPicker from "../../utils/ColourPicker";

const highlightFilter = new GlowFilter({ distance: 15, outerStrength: 1, innerStrength: 1, color: 0xfde90d });
const blackTexture = Texture.fromBuffer(new Uint8Array([0, 0, 0, 255]), 1, 1);

const getTexture = ({ backgroundGraphic }: MapTileProps) => {
    if (backgroundGraphic !== undefined) {
        return Texture.fromBuffer(backgroundGraphic, 16, 16);
    } else {
        return blackTexture;
    }
}

const handleEvent = (event: FederatedPointerEvent, { onClickRef, highlight, moving, x, y }: MapTileProps) => {
    if ((event.pointerType === "mouse" && event.button !== 0) || moving.current) {
        return;
    }
    const coordinate = { x, y };
    onClickRef.current && onClickRef.current(coordinate, highlight);
};

const MapTile = PixiComponent("MapTile", {
    create: () => {
        const mapTile = new Sprite();
        mapTile.cullable = true;
        return mapTile;
    },
    applyProps: (instance: Sprite, _oldProps: MapTileProps, newProps: MapTileProps) => {
        const { x, y, width, height, alpha, highlight, hue, saturation, lightness } = newProps;

        instance.texture = getTexture(newProps);
        instance.width = width + 1;
        instance.height = height + 1;

        const overlay: ColorOverlayFilter = colourPicker.getColourOverlay(hue, saturation, lightness, alpha);

        instance.filters = [overlay];

        if (highlight) {
            instance.filters.push(highlightFilter);
        } else {
            instance.filters.splice(1);
        }

        instance.x = x - 0.5;
        instance.y = y - 0.5;
        if (newProps.onClickRef.current) {
            instance.eventMode = "static";
            instance.on("pointertap", (event) => handleEvent(event, newProps));
        }
    }
});

export default MapTile;
