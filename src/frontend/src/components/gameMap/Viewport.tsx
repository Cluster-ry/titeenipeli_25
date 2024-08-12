import React from "react";
import * as PIXI from "pixi.js";
import { PixiComponent, useApp } from "@pixi/react";
import { Viewport as PixiViewport } from "pixi-viewport";

export interface ViewportProps {
  width: number;
  height: number;
  maxWidth: number;
  maxHeight: number;
  children?: React.ReactNode;
}

export interface PixiComponentViewportProps extends ViewportProps {
  app: PIXI.Application;
}

const PixiComponentViewport = PixiComponent("Viewport", {
  create: (props: PixiComponentViewportProps) => {
    const viewport = new PixiViewport({
      screenWidth: props.width,
      screenHeight: props.height,
      worldWidth: props.maxWidth,
      worldHeight: props.maxHeight,
      events: props.app.renderer.events
    });
    viewport
      .drag(
        {
          underflow: "center"
        }
      )
      .pinch()
      .wheel()
      .decelerate({
        friction: 0.955,
        minSpeed: 0.05
      })
      .bounce(
        {
          friction: 0.5,
          time: 500
        }
      )
      .clampZoom({
        minScale: 0.5,
        maxScale: 2
      });

    return viewport;
  }
});

const Viewport = (props: ViewportProps) => {
  const app = useApp();
  (globalThis as any).__PIXI_APP__ = app;
  (globalThis as any).__PIXI_STAGE__ = app.stage;
  (globalThis as any).__PIXI_RENDERER__ = app.renderer;
  return <PixiComponentViewport app={app} {...props} />;
};

export default Viewport;
