import { PixiComponent } from "@pixi/react";
import { Container, FederatedPointerEvent, Graphics, Sprite, Texture } from "pixi.js";
import MapTileProps from "../../models/MapTileProps";
import { ColorOverlayFilter, GlowFilter } from "pixi-filters";
import colourPicker from "../../utils/ColourPicker";

const highlightFilter = new GlowFilter({ distance: 15, outerStrength: 1, innerStrength: 1, color: 0xfde90d });

const getTexture = ({ backgroundGraphic, width, height }: MapTileProps) => {
    if (backgroundGraphic !== undefined) {
        return Texture.fromBuffer(backgroundGraphic, width, height);
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
        container.cullable = true;
        return container;
    },
    applyProps: (instance: Graphics, _oldProps: MapTileProps, newProps: MapTileProps) => {
        const { x, y, width, height, alpha, highlight, hue, saturation, lightness } = newProps;
        instance.removeChildren()

        const bgSprite = new Sprite(getTexture(newProps));
        Object.assign(bgSprite, {
            width: width + 0.05,
            height: height + 0.05,
        });

        const overlay: ColorOverlayFilter = colourPicker.getColourOverlay(hue, saturation, lightness, alpha);

        instance.addChild(bgSprite);
        instance.filters = [overlay];

        if (highlight) {
            instance.filters.push(highlightFilter);
        } else {
            instance.filters.splice(1);
        }

        instance.x = x;
        instance.y = y;
        if (newProps.onClickRef.current) {
            instance.eventMode = "static";
            instance.on("pointertap", (event) => handleEvent(event, newProps));
        }
    }
});

export default MapTile;
