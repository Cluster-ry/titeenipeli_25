import { forwardRef, useImperativeHandle, useRef } from "react";
import { type EffectHandle } from "./support/EffectBatch";
import { Container } from "@pixi/react";
import { Coordinate } from "../../../models/Coordinate";
import { ConquerEffect } from "./effects/ConquerEffect";
import { ForbiddenEffect } from "./effects/ForbiddenEffect";

export type EffectContainerHandle = {
    forbiddenEffect: (coordinate: Coordinate) => void;
    conqueredEffect: (coordinate: Coordinate) => void;
};

type Props = object;

export const EffectContainer = forwardRef<EffectContainerHandle, Props>((_, ref) => {
    const forbiddenRef = useRef<EffectHandle>(null);
    const conquerRef = useRef<EffectHandle>(null);

    useImperativeHandle(ref, () => ({
        forbiddenEffect(coordinate: Coordinate) {
            forbiddenRef.current?.effect(coordinate);
        },
        conqueredEffect(coordinate: Coordinate) {
            conquerRef.current?.effect(coordinate);
        },
    }));

    return (
        <Container>
            <ForbiddenEffect ref={forbiddenRef} />
            <ConquerEffect ref={conquerRef} />
        </Container>
    );
});
