import * as PIXI from "pixi.js";
import { Rectangle } from "pixi.js";
import { PixiComponent, useApp } from "@pixi/react";
import { Viewport as PixiViewport } from "pixi-viewport";
import { mapConfig } from "./MapConfig";

import ViewportProps from "../../models/ViewportProps";
import ViewportBoundingBox from "../../models/ViewportBoundingBox.ts";

export interface PixiComponentViewportProps extends ViewportProps {
    app: PIXI.Application;
}

const bounceRectangle = new Rectangle();
const padding = 300;

const computeDimensions = (boundingBox: ViewportBoundingBox, screenWidth: number, screenHeight: number) => {
    const tempWidth = Math.abs(boundingBox.maxX - boundingBox.minX) * mapConfig.PixelSize + padding;
    const tempHeight = Math.abs(boundingBox.maxY - boundingBox.minY) * mapConfig.PixelSize + padding;
    const maxWidth = Math.max(tempWidth, screenWidth);
    const maxHeight = Math.max(tempHeight, screenHeight);
    const startX = boundingBox.minX - (maxWidth / 2);
    const startY = boundingBox.minY - (maxHeight / 2);
    const result = {
        maxWidth, maxHeight, startX, startY
    }
    return result;
};

const setBounceRectangle = (width: number, height: number, startX: number, startY: number) => {
    bounceRectangle.width = width
    bounceRectangle.height = height,
    bounceRectangle.x = startX
    bounceRectangle.y = startY;
    return bounceRectangle;
};

const PixiComponentViewport = PixiComponent("Viewport", {
    create: (props: PixiComponentViewportProps) => {
        const { maxWidth, maxHeight, startX, startY } = computeDimensions(props.boundingBox, props.width, props.height);
        const viewport = new PixiViewport({
            screenWidth: props.width,
            screenHeight: props.height,
            worldWidth: maxWidth,
            worldHeight: maxHeight,
            events: props.app.renderer.events,
        });
        const bounceRectangle = setBounceRectangle(maxWidth, maxHeight, startX, startY);
        const centerX = bounceRectangle.x + (bounceRectangle.width / 2);
        const centerY = bounceRectangle.y + (bounceRectangle.height / 2);
        viewport
            .drag({
                underflow: "center",
            })
            .pinch()
            .wheel()
            .decelerate({
                friction: 0.955,
                minSpeed: 0.05,
            })
            .bounce({
                friction: 0.5,
                time: 500,
                bounceBox: bounceRectangle,
            })
            .clampZoom({
                minScale: 0.1,
                maxScale: 15,
            })
            .moveCenter(centerX, centerY)
            .fit();
        viewport.options.disableOnContextMenu = true;
        viewport.options.stopPropagation = true;

        viewport.addEventListener("moved", props.onMoveStart);

        return viewport;
    },

    applyProps(viewport: PixiViewport, _oldProps: PixiComponentViewportProps, newProps: PixiComponentViewportProps) {
        const { maxWidth, maxHeight, startX, startY } = computeDimensions(newProps.boundingBox, newProps.width, newProps.height);
        setBounceRectangle(maxWidth, maxHeight, startX, startY);
        viewport.worldWidth = maxWidth;
        viewport.worldHeight = maxHeight;
    },
});

const Viewport = (props: ViewportProps) => {
    const app = useApp();
    return <PixiComponentViewport app={app} {...props} />;
};

export default Viewport;
