import { Sprite } from "@pixi/react";
import { Texture } from "pixi.js";
import ForegroundRectangleProps from "../../models/ForegroundRectangleProps";

const ForegroundRectangle = ({ color, x, y, width, height }: ForegroundRectangleProps) => {
    return (
        <Sprite
            position={{ x: x, y: y }}
            tint={`hsl(${color.hue}, ${color.saturation}%, ${color.lightness}%)`}
            alpha={color.alpha ?? 0.75}
            texture={Texture.WHITE}
            width={width}
            height={height}
            cullable={true}
            eventMode="none"
        />
    );
};

export default ForegroundRectangle;
