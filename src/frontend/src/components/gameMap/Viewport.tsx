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
const padding = 500;
const test = new PIXI.Graphics();
test.alpha = 0.5;

const computeDimensions = (boundingBox: ViewportBoundingBox, screenWidth: number, screenHeight: number) => {
    const worldMinX = boundingBox.minX * mapConfig.PixelSize;
    const worldMinY = boundingBox.minY * mapConfig.PixelSize;

    const worldWidth = Math.abs(boundingBox.maxX * mapConfig.PixelSize - worldMinX);
    const worldHeight = Math.abs(boundingBox.maxY * mapConfig.PixelSize - worldMinY);

    const boundaryWidth = Math.max(worldWidth + padding, screenWidth);
    const boundaryHeight = Math.max(worldHeight + padding, screenHeight);
    const boundaryX = worldMinX - (boundaryWidth > screenWidth ? (padding / 2) : (screenWidth - worldWidth) / 2);
    const boundaryY = worldMinY - (boundaryHeight > screenHeight ? (padding / 2) : (screenHeight - worldHeight) / 2);

    return { worldWidth, worldHeight, boundaryWidth, boundaryHeight, worldMinX, worldMinY, boundaryX, boundaryY };
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
        const { boundaryWidth, boundaryHeight, boundaryX, boundaryY } = computeDimensions(props.boundingBox, props.width, props.height);
        const viewport = new PixiViewport({
            screenWidth: props.width,
            screenHeight: props.height,
            worldWidth: boundaryWidth,
            worldHeight: boundaryHeight,
            events: props.app.renderer.events,
        });
        const bounceRectangle = setBounceRectangle(boundaryWidth, boundaryHeight, boundaryX, boundaryY);
        const centerX = boundaryX + (boundaryWidth / 2);
        const centerY = boundaryY + (boundaryHeight / 2);

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
                minScale: 0.3,
                maxScale: 15,
            })
            .moveCenter(centerX, centerY)
        viewport.options.disableOnContextMenu = true;
        viewport.options.stopPropagation = true;

        test.beginFill(0xFFFF00);

        // set the line style to have a width of 5 and set the color to red
        test.lineStyle(5, 0xFF0000);
        // draw a rectangle
        test.drawRect(boundaryX, boundaryY, boundaryWidth, boundaryHeight);
        viewport.addChild(test);

        viewport.addEventListener("moved", props.onMoveStart);

        return viewport;
    },

    applyProps(viewport: PixiViewport, _oldProps: PixiComponentViewportProps, newProps: PixiComponentViewportProps) {
        const { boundaryX, boundaryY, boundaryWidth, boundaryHeight } = computeDimensions(newProps.boundingBox, newProps.width, newProps.height);
        setBounceRectangle(boundaryWidth, boundaryHeight, boundaryX, boundaryY);
        viewport.worldWidth = boundaryWidth;
        viewport.worldHeight = boundaryHeight;
        test.clear();
        test.beginFill(0xFFFF00);
        // set the line style to have a width of 5 and set the color to red
        test.lineStyle(5, 0xFF0000);
        // draw a rectangle
        test.drawRect(boundaryX, boundaryY, boundaryWidth, boundaryHeight);
    },
});

const Viewport = (props: ViewportProps) => {
    const app = useApp();
    return <PixiComponentViewport app={app} {...props} />;
};

export default Viewport;
