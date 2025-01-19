import { forwardRef } from "react";
import { Coordinate } from "../../../../models/Coordinate";
import { EffectBatch, EffectHandle } from "../support/EffectBatch";
import { EffectItem } from "../support/EffectSprite";
import { getPixelCenterCoordinate } from "./common";
import forbidden from "../../../../assets/forbidden.png";

const sprite = forbidden;
const duration = 3;
const count = 1;

const startEffect = (coordinate: Coordinate): EffectItem => ({
    speed: 5,
    turningSpeed: 1.5,
    direction: Math.random() * Math.PI * 2,
    position: getPixelCenterCoordinate(coordinate),
    scale: 0.05,
    rotation: 0,
    duration: 0,
    anchor: [0.5, 0.5],
});

const updateEffect = (item: EffectItem): EffectItem => ({
    ...item,
    scale:
        item.scale +
        (item.duration < 2 ? Math.sin(item.duration * item.scale) : -Math.sin(item.duration * item.scale)) * 0.01,
    position: {
        x: item.position.x + Math.sin(item.direction) * (item.speed * item.scale),
        y: item.position.y + Math.cos(item.direction) * (item.speed * item.scale),
    },
    rotation: -item.direction + Math.PI,
    direction: item.direction + item.turningSpeed * 0.02,
});

export const ForbiddenEffect = forwardRef<EffectHandle>((_, ref) => {
    return (
        <EffectBatch
            ref={ref}
            sprite={sprite}
            count={count}
            duration={duration}
            startEffect={startEffect}
            updateEffect={updateEffect}
        />
    );
});
