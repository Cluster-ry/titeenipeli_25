import { PixiComponent } from "@pixi/react";
import { Color, Container, FederatedPointerEvent, Graphics, Sprite, Texture } from "pixi.js";
import MapTileProps from "../../models/MapTileProps";
import { ColorOverlayFilter, GlowFilter } from "pixi-filters";

const backgroundGraphicSize = 32;

const getColor = ({hue, saturation, lightness}: MapTileProps) => {
    if (hue !== undefined && saturation !== undefined && lightness !== undefined) {
        const pixiColor = new Color(`hsl(${hue}, ${saturation}%, ${lightness}%)`);
        return [pixiColor.red, pixiColor.green, pixiColor.blue];
    } else {
        return [0, 0, 0];
    }
};

const getTexture = ({ backgroundGraphic }: MapTileProps) => {
    if (backgroundGraphic !== undefined) {
        return Texture.fromBuffer(backgroundGraphic, backgroundGraphicSize, backgroundGraphicSize);
    } else {
        return Texture.WHITE;
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
        const container = new Container();
        return container;
    },
    applyProps: (instance: Graphics, _oldProps: MapTileProps, newProps: MapTileProps) => {
        const { x, y, width, height, alpha, highlight } = newProps;
        instance.removeChildren()

        const bgSprite = new Sprite(getTexture(newProps));
        Object.assign(bgSprite, {
            width,
            height,
        });

        const overlay = new ColorOverlayFilter(getColor(newProps), alpha ?? 0.75);

        instance.addChild(bgSprite);
        instance.filters = [overlay];

        if (highlight) {
            const highlightFilter = new GlowFilter({ distance: 15, outerStrength: 1, innerStrength: 1, color: 0xfde90d });
            instance.filters.push(highlightFilter);
        } else {
            instance.filters.splice(1);
        }

        instance.x = x;
        instance.y = y;
        if (newProps.onClickRef.current) {
            instance.eventMode = "static";
            instance.on("pointerdown", (event) => handleEvent(event, newProps));
        }
    }
});

export default MapTile;
