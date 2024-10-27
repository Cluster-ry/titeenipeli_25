import { forwardRef, useImperativeHandle, useRef } from "react";
import { Coordinate } from "../../models/Coordinate";
import { EffectBatch, type EffectHandle } from "./EffectBatch";
import { type EffectItem } from "../EffectSprite";
import { mapConfig } from "./MapConfig";
import { Container } from "@pixi/react";

export type EffectContainerHandle = {
  forbiddenEffect: (coordinate: Coordinate) => void;
  conqueredEffect: (coordinate: Coordinate) => void;
};

type Props = object;

const forbiddenSprite = "src/assets/forbidden.png";
const conquerSprite = "src/assets/logo.png";

const forbiddenDuration = 7;
const conquerDuration = 7;

const getMapCoordinates = ({ x, y }: Coordinate): Coordinate => ({
  x: x * mapConfig.PixelSize,
  y: y * mapConfig.PixelSize,
});

const forbiddenStartEffect = (coordinate: Coordinate): EffectItem => ({
    speed: (20 + Math.random() * 2) * 0.2,
    turningSpeed: Math.random() - 0.25,
    direction: Math.random() * Math.PI * 2,
    position: getMapCoordinates(coordinate),
    scale: 0.025 + Math.random() * 0.1,
    rotation: 0,
    duration: 0,
});

const conquerStartEffect = (coordinate: Coordinate): EffectItem => ({
    speed: (20 + Math.random() * 2) * 0.2,
    turningSpeed: Math.random() - 0.25,
    direction: Math.random() * Math.PI * 2,
    position: getMapCoordinates(coordinate),
    scale: 0.025 + Math.random() * 0.1,
    rotation: 0,
    duration: 0,
});

const forbiddenUpdateEffect = (item: EffectItem): EffectItem => ({
    ...item,
    scale: item.scale + (item.duration < 4 ? Math.sin(item.duration * item.scale) : -Math.sin(item.duration * item.scale)) * 0.005,
    position: {
        x: item.position.x + Math.sin(item.direction) * (item.speed * item.scale),
        y: item.position.y + Math.cos(item.direction) * (item.speed * item.scale),
    },
    rotation: -item.direction + Math.PI,
    direction: item.direction + item.turningSpeed * 0.02,
});

const conquerUpdateEffect = (item: EffectItem): EffectItem => ({
  ...item,
  scale: item.scale + (item.duration < 4 ? Math.sin(item.duration * item.scale) : -Math.sin(item.duration * item.scale)) * 0.005,
  position: {
    x: item.position.x + Math.sin(item.direction) * (item.speed * item.scale),
    y: item.position.y + Math.cos(item.direction) * (item.speed * item.scale),
  },
  rotation: -item.direction + Math.PI,
  direction: item.direction + item.turningSpeed * 0.02,
});

export const EffectContainer = forwardRef<EffectContainerHandle, Props>(
  (_, ref) => {
    const forbiddenRef = useRef<EffectHandle>(null);
    const conquerRef = useRef<EffectHandle>(null);

    useImperativeHandle(ref, () => ({
      forbiddenEffect(coordinate: Coordinate) {
        console.log("Forbidden");
        forbiddenRef.current?.effect(coordinate);
      },
      conqueredEffect(coordinate: Coordinate) {
        console.log("Conquer");
        conquerRef.current?.effect(coordinate);
      },
    }));

    return (
      <Container>
        <EffectBatch
          ref={forbiddenRef}
          sprite={forbiddenSprite}
          count={1}
          duration={forbiddenDuration}
          startEffect={forbiddenStartEffect}
          updateEffect={forbiddenUpdateEffect}
        />
        <EffectBatch
          ref={conquerRef}
          sprite={conquerSprite}
          count={5}
          duration={conquerDuration}
          startEffect={conquerStartEffect}
          updateEffect={conquerUpdateEffect}
        />
      </Container>
    );
  }
);
