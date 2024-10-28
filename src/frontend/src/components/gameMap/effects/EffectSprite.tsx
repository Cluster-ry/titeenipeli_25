import { FC } from "react";
import { Sprite } from "@pixi/react";
import { ColorSource } from "pixi.js";
import { Coordinate } from "../../../models/Coordinate";
import { mapConfig } from "../MapConfig";

export type EffectItem = {
    scale: number,
    speed: number,
    rotation: number,
    position: Coordinate,
    direction: number,
    turningSpeed: number,
    duration: number,
    tint?: ColorSource,
    anchor?: [number, number]
}

export const EffectSprite: FC<EffectItem & { sprite: string }> = (props) => (
    <Sprite
        width={mapConfig.PixelSize}
        height={mapConfig.PixelSize}
        cullable={true}
        eventMode="dynamic"
        image={props.sprite}
        {...props}
    />)