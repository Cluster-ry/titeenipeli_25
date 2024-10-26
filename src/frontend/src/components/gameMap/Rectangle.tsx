import { Sprite } from "@pixi/react";
import { Texture } from "pixi.js";
import RectangleProps from "../../models/RectangleProps";

const Rectangle = ({ color, onClick, x, y, width, height, isOwn, isSpawn }: RectangleProps) => {
    // When a client clicks a pixel
    const handleClick = () => {
        onClick({ x: x, y: y });
    };

    let calculatedLightness = color.lightness;
    if (isOwn) {
        calculatedLightness += 10;
    }

    if (isSpawn) {
        calculatedLightness -= 20;
    }

    return (
        <Sprite
            position={{ x: x, y: y }}
            tint={`hsla(${color.hue}, ${color.saturation}%, ${calculatedLightness}%, ${color.alpha ?? 100}%)`}
            texture={Texture.WHITE}
            width={width}
            height={height}
            cullable={true}
            eventMode="dynamic"
            pointertap={handleClick}
        />
    );
};

export default Rectangle;
