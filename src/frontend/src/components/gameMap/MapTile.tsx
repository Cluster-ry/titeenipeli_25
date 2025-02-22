import { PixiComponent } from "@pixi/react";
import { Color, ColorMatrixFilter, Container, FederatedPointerEvent, Graphics, Sprite, Texture } from "pixi.js";
import RectangleProps from "../../models/RectangleProps";

const backgroundGraphicSize = 32;

const getColor = ({hue, saturation, lightness}: RectangleProps) => {
    if (hue !== undefined && saturation !== undefined && lightness !== undefined) {
        const pixiColor = new Color(`hsl(${hue}, ${saturation}%, ${lightness}%)`);
        return [pixiColor.red, pixiColor.green, pixiColor.blue];
    } else {
        return [0, 0, 0];
    }
};

const getTexture = ({ backgroundGraphic }: RectangleProps) => {
    if (backgroundGraphic !== undefined) {
        return Texture.fromBuffer(backgroundGraphic, backgroundGraphicSize, backgroundGraphicSize);
    } else {
        return Texture.WHITE;
    }
}

const handleEvent = (event: FederatedPointerEvent, { onClickRef, highlight, moving, x, y }: RectangleProps) => {
    if ((event.pointerType === "mouse" && event.button !== 0) || moving.current) {
        return;
    }
    const coordinate = { x, y };
    onClickRef.current && onClickRef.current(coordinate, highlight);
};

const MapTile = PixiComponent("MapTile", {
    create: () => new Container(),
    applyProps: (instance: Graphics, _oldProps: RectangleProps, newProps: RectangleProps) => {
        console.log("Perse");
        const { x, y, width, height, alpha, highlight } = newProps;
        instance.removeChildren()

        const bgSprite = new Sprite(getTexture(newProps));
        Object.assign(bgSprite, {
            width,
            height,
        });

        const overlay = new Graphics();
        overlay.beginFill(getColor(newProps), alpha ?? 0.75)
        .drawRect(0, 0, newProps.width, newProps.height)
        .endFill();

        instance.addChild(bgSprite);
        instance.addChild(overlay);

        if (highlight) {
            const highlightFilter = new ColorMatrixFilter();
            highlightFilter.brightness(0.8, true);
            instance.filters = [highlightFilter];
        } else {
            instance.filters = null;
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
