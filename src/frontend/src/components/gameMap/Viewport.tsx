import React from "react";
import * as PIXI from "pixi.js";
import { Rectangle } from 'pixi.js';
import { PixiComponent, useApp } from "@pixi/react";
import { Viewport as PixiViewport } from "pixi-viewport";
import { mapConfig } from "./MapConfig";

export interface ViewportBoundigBox {
  minX: number;
  minY: number;
  maxX: number;
  maxY: number;
}

export interface ViewportProps {
  width: number;
  height: number;
  boundingBox: ViewportBoundigBox;
  children?: React.ReactNode;
}

export interface PixiComponentViewportProps extends ViewportProps {
  app: PIXI.Application;
}

const extraPadding = 20

const PixiComponentViewport = PixiComponent("Viewport", {
  create: (props: PixiComponentViewportProps) => {
    const maxWidth = (props.boundingBox.minX / 2 + props.boundingBox.maxX) * mapConfig.PixelSize
    const maxHeight = (props.boundingBox.minY / 2 + props.boundingBox.maxY) * mapConfig.PixelSize

    const viewport = new PixiViewport({
      screenWidth: props.width,
      screenHeight: props.height,
      worldWidth: maxWidth,
      worldHeight: maxHeight,
      events: props.app.renderer.events,
    });
    const bounceRectangle = new Rectangle()
    bounceRectangle.x = (props.boundingBox.minX - extraPadding) * mapConfig.PixelSize
    bounceRectangle.y = (props.boundingBox.minY - extraPadding) * mapConfig.PixelSize
    bounceRectangle.width = (Math.abs(props.boundingBox.minX) + props.boundingBox.maxX + extraPadding) * mapConfig.PixelSize
    bounceRectangle.height = (Math.abs(props.boundingBox.minY) + props.boundingBox.maxY + extraPadding) * mapConfig.PixelSize
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
      });

    return viewport;
  },
});

const Viewport = (props: ViewportProps) => {
  const app = useApp();
  return <PixiComponentViewport app={app} {...props} />;
};

export default Viewport;
