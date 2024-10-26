import * as PIXI from "pixi.js";
import { Rectangle } from 'pixi.js';
import { PixiComponent, useApp } from "@pixi/react";
import { Viewport as PixiViewport } from "pixi-viewport";
import { mapConfig } from "./MapConfig";

import ViewportProps from "../../models/ViewportProps";
import ViewportBoundigBox from "../../models/ViewportBoundigBox";


export interface PixiComponentViewportProps extends ViewportProps {
  app: PIXI.Application;
}

const extraPadding = 100

const PixiComponentViewport = PixiComponent("Viewport", {
  create: (props: PixiComponentViewportProps) => {
    const {maxWidth, maxHeight} = computeDimensions(props.boundingBox)

    const viewport = new PixiViewport({
      screenWidth: props.width,
      screenHeight: props.height,
      worldWidth: maxWidth,
      worldHeight: maxHeight,
      events: props.app.renderer.events,
    });
    const bounceRectangle = computeBounceRectangle(props.boundingBox)
    const centerX = (bounceRectangle.x + bounceRectangle.width) / 2
    const centerY = (bounceRectangle.y + bounceRectangle.width) / 2
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
        bounceBox: bounceRectangle
      })
      .clampZoom({
        minScale: 0.5,
        maxScale: 2,
      })
      .moveCenter(centerX, centerY);

    return viewport;
  },

  applyProps(viewport: PixiViewport, _: PixiComponentViewportProps, newProps: PixiComponentViewportProps) {
    const {maxWidth, maxHeight} = computeDimensions(newProps.boundingBox)
    const bounceRectangle = computeBounceRectangle(newProps.boundingBox)
    viewport.worldWidth = maxWidth
    viewport.worldHeight = maxHeight
    viewport.bounce({
      friction: 0.5,
      time: 500,
      bounceBox: bounceRectangle
    })
  }
});

const computeDimensions = (boundingBox: ViewportBoundigBox) => {
  const maxWidth = (boundingBox.minX / 2 + boundingBox.maxX) * mapConfig.PixelSize
  const maxHeight = (boundingBox.minY / 2 + boundingBox.maxY) * mapConfig.PixelSize
  return {maxWidth, maxHeight}
}

const computeBounceRectangle = (boundingBox: ViewportBoundigBox) => {
  const bounceRectangle = new Rectangle()
  bounceRectangle.x = (boundingBox.minX - extraPadding) * mapConfig.PixelSize
  bounceRectangle.y = (boundingBox.minY - extraPadding) * mapConfig.PixelSize
  bounceRectangle.width = (Math.abs(boundingBox.minX) + boundingBox.maxX + extraPadding) * mapConfig.PixelSize
  bounceRectangle.height = (Math.abs(boundingBox.minY) + boundingBox.maxY + extraPadding) * mapConfig.PixelSize
  return bounceRectangle
}

const Viewport = (props: ViewportProps) => {
  const app = useApp();
  return <PixiComponentViewport app={app} {...props} />;
};

export default Viewport;
