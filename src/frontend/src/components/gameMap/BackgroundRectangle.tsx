import { Sprite } from "@pixi/react";
import { Texture } from "pixi.js";
import BackgroundRectangleProps from "../../models/BackgroundRectangleProps";

const backgroundGraphicSize = 32;

const BackgroundRectangle = ({ x, y, width, height, backgroundGraphic, onClick }: BackgroundRectangleProps) => {
    // When a client clicks a pixel
    const handleClick = () => {
        onClick({ x: x, y: y });
    };

    const texture = Texture.fromBuffer(backgroundGraphic, backgroundGraphicSize, backgroundGraphicSize);

    return (
        <Sprite
            position={{ x: x, y: y }}
            image="test"
            width={width}
            height={height}
            cullable={true}
            texture={texture}
            eventMode="static"
            pointertap={handleClick}
        />
    );
};

export default BackgroundRectangle;
