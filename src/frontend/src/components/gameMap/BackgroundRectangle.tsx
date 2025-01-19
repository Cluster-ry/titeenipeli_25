import { Container, Sprite, withFilters } from "@pixi/react";
import { FederatedPointerEvent, Texture } from "pixi.js";
import BackgroundRectangleProps from "../../models/BackgroundRectangleProps";
import { useInputEventStore } from "../../stores/inputEventStore";
import { GlowFilter } from "pixi-filters";

const backgroundGraphicSize = 32;

const Filters = withFilters(Container, {
    glow: GlowFilter
});

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

        onClick({ x, y });
    };

    const texture = Texture.fromBuffer(backgroundGraphic, backgroundGraphicSize, backgroundGraphicSize);

    return (
        <Filters glow={{ enabled: false, outerStrength: 5, innerStrength: 2, color: 0xfde90d, alpha: 1, knockout: false }}>
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
        </Filters>
    );
};

export default BackgroundRectangle;
