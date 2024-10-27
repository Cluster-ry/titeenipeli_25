import { Sprite } from "@pixi/react";
import { mapConfig } from "./gameMap/MapConfig";
import { FC } from "react";
import { Coordinate } from "../models/Coordinate";

export type EffectItem = {
    scale: number,
    speed: number,
    rotation: number,
    position: Coordinate,
    direction: number,
    turningSpeed: number,
    duration: number,
}

export const EffectSprite: FC<EffectItem & { sprite: string }> = (props) => (
    <Sprite
        width={mapConfig.PixelSize}
        height={mapConfig.PixelSize}
        cullable={true}
        eventMode="dynamic"
        anchor={[1, 1]}
        image={props.sprite}
        {...props}
    />
)