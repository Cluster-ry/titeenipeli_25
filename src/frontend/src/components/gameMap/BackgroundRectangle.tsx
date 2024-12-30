import { Sprite } from "@pixi/react";
import { FederatedPointerEvent, Texture } from "pixi.js";
import BackgroundRectangleProps from "../../models/BackgroundRectangleProps";
import { useInputEventStore } from "../../stores/inputEventStore";

const backgroundGraphicSize = 32;

const BackgroundRectangle = ({ x, y, width, height, backgroundGraphic, onClick }: BackgroundRectangleProps) => {
    const inputEventStore = useInputEventStore();

    // When a client clicks a pixel
    const handleEvent = (event: FederatedPointerEvent) => {
        if (event.pointerType === "mouse" && event.button !== 0) {
            return;
        }

        if (inputEventStore.moving || inputEventStore.moveEnded.getTime() > new Date().getTime() - 300) {
            return;
        }

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
            mousedown={handleEvent}
            tap={handleEvent}
        />
    );
};

export default BackgroundRectangle;
