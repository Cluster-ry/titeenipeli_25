import { forwardRef, useImperativeHandle, useState } from "react";
import { Coordinate } from "../../models/Coordinate";
import { ParticleContainer, useTick } from "@pixi/react";
import { type EffectItem, EffectSprite } from "../EffectSprite";

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
    }
}

const getKey = ({x, y}: Coordinate) => `${x}${y}-${Math.random()}`;

export const EffectBatch = forwardRef<EffectHandle, Props>((props, ref) => {
    const [ effects, setEffects ] = useState<EffectItem[]>([]);

    useImperativeHandle(ref, () => ({
      effect(coordinate: Coordinate) {
            // TODO: Activate "Action forbidden" effect at chosen coordinates
            console.log("mroo");
            const temp: EffectItem[] = [];
            for (let i = 0; i < props.count; i++) {
              temp.push(props.startEffect(coordinate));
            }
            setEffects(previous => [...previous, ...temp]);
        }
    }), [setEffects])

    useTick((delta) => {
      console.log("moro");
      setEffects(prevItems => {
        return prevItems.map(item => ({...props.updateEffect(item), duration: item.duration += (delta * 0.1)})).filter(item => item.duration < props.duration);
      });
    }, effects.length > 0);

    return <ParticleContainer properties={config.properties}>
            {effects.map(entry => <EffectSprite key={getKey(entry.position)} sprite={props.sprite} {...entry} />)}
        </ParticleContainer>
});