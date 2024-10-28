import { forwardRef, useImperativeHandle, useRef } from "react";
import { EffectBatch, type EffectHandle } from "./EffectBatch";
import { Container } from "@pixi/react";
import { Coordinate } from "../../../models/Coordinate";
import { conquerStartEffect, conquerUpdateEffect, forbiddenStartEffect, forbiddenUpdateEffect } from "./effects";

export type EffectContainerHandle = {
  forbiddenEffect: (coordinate: Coordinate) => void;
  conqueredEffect: (coordinate: Coordinate) => void;
};

type Props = object;

const forbiddenSprite = "src/assets/forbidden.png";
const conquerSprite = "src/assets/bucket.png";

const forbiddenDuration = 3;
const conquerDuration = 3;

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
          count={10}
          duration={conquerDuration}
          startEffect={conquerStartEffect}
          updateEffect={conquerUpdateEffect}
        />
      </Container>
    );
  }
);
