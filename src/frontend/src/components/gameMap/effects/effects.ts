import { Coordinate } from "../../../models/Coordinate";
import { mapConfig } from "../MapConfig";
import { EffectItem } from "./EffectSprite";

export const getMapCoordinates = ({ x, y }: Coordinate): Coordinate => ({
    x: x * mapConfig.PixelSize + 5,
    y: y * mapConfig.PixelSize + 5,
});

export const forbiddenStartEffect = (coordinate: Coordinate): EffectItem => ({
    speed: 5,
    turningSpeed: 1.5,
    direction: Math.random() * Math.PI * 2,
    position: getMapCoordinates(coordinate),
    scale: 0.05,
    rotation: 0,
    duration: 0,
    anchor: [0.5, 0.5],
});

export const conquerStartEffect = (coordinate: Coordinate): EffectItem => ({
    speed: (40 + Math.random() * 2) * 0.2,
    turningSpeed: Math.random() * 2,
    direction: Math.random() * Math.PI * 2,
    position: getMapCoordinates(coordinate),
    scale: 0.01 + Math.random() * 0.04,
    rotation: 0,
    duration: 0,
    anchor: [0.5, 0.5],
    tint: Math.random() * 0xffffff,
});

export const forbiddenUpdateEffect = (item: EffectItem): EffectItem => ({
    ...item,
    scale: item.scale + (item.duration < 2 ? Math.sin(item.duration * item.scale) : -Math.sin(item.duration * item.scale)) * 0.01,
    position: {
        x: item.position.x + Math.sin(item.direction) * (item.speed * item.scale),
        y: item.position.y + Math.cos(item.direction) * (item.speed * item.scale),
    },
    rotation: -item.direction + Math.PI,
    direction: item.direction + item.turningSpeed * 0.02,
});

export const conquerUpdateEffect = (item: EffectItem): EffectItem => ({
    ...item,
    scale: item.scale + (item.duration < 2 ? Math.sin(item.duration * item.scale) : -Math.sin(item.duration * item.scale * 3)) * 0.005,
    position: {
        x: item.position.x + Math.sin(item.direction) * (item.speed * item.scale),
        y: item.position.y + Math.cos(item.direction) * (item.speed * item.scale),
    },
    rotation: -item.direction + Math.PI,
    direction: item.direction + item.turningSpeed * 0.02,
});