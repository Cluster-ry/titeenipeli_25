import { forwardRef, useImperativeHandle, useState } from "react";
import { ParticleContainer, useTick } from "@pixi/react";
import { Coordinate } from "../../../../models/Coordinate";
import { type EffectItem, EffectSprite } from "./EffectSprite";

export type EffectHandle = {
    effect: (coordinate: Coordinate) => void;
};

type Props = {
    sprite: string;
    count: number;
    duration: number;
    startEffect: (coordinate: Coordinate) => EffectItem;
    updateEffect: (item: EffectItem) => EffectItem;
};

const config = {
    properties: {
        position: true,
        rotation: true,
        scale: true,
        uvs: false,
        alpha: false,
    },
};

const getKey = ({ x, y }: Coordinate, index: number) => `${x}-${y}-${index}-${new Date().getTime()}`;

export const EffectBatch = forwardRef<EffectHandle, Props>((props, ref) => {
    const [effects, setEffects] = useState<EffectItem[]>([]);

    useImperativeHandle(
        ref,
        () => ({
            effect(coordinate: Coordinate) {
                const temp: EffectItem[] = [];
                for (let i = 0; i < props.count; i++) {
                    temp.push(props.startEffect(coordinate));
                }
                setEffects((previous) => [...previous, ...temp]);
            },
        }),
        [setEffects],
    );

    useTick((delta) => {
        setEffects((prevItems) => {
            return prevItems.flatMap((item) => {
                if (item.duration < props.duration) {
                    return { ...props.updateEffect(item), duration: (item.duration += delta * 0.1) };
                }
                return [];
            });
        });
    }, effects.length > 0);

    return (
        <ParticleContainer properties={config.properties}>
            {effects.map((entry, index) => (
                <EffectSprite key={getKey(entry.position, index)} sprite={props.sprite} {...entry} />
            ))}
        </ParticleContainer>
    );
});
