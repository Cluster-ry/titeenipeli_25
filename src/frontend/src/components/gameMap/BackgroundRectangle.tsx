import { Container, Sprite, withFilters } from "@pixi/react";
import { FederatedPointerEvent, Texture } from "pixi.js";
import BackgroundRectangleProps from "../../models/BackgroundRectangleProps";
import { GlowFilter } from "pixi-filters";
import { useCallback, useMemo, useRef } from "react";

const backgroundGraphicSize = 32;

const Filters = withFilters(Container, {
    glow: GlowFilter,
});

const BackgroundRectangle = ({
    x,
    y,
    width,
    height,
    backgroundGraphic,
    highlight,
    onClick,
}: BackgroundRectangleProps) => {
    const mouseMoving = useRef<boolean>(false); 

    // When a client clicks a pixel
    const mouseMoved = useCallback(async () => {
        if (mouseMoving.current) return;
        const moved = new Promise(resolve => {
            mouseMoving.current = true;
            setTimeout(() => {
                mouseMoving.current = false;
                resolve(true);
            }, 10);
        });
        await moved;
    }, []);

    const handleEvent = useCallback((event: FederatedPointerEvent) => {
        if (event.pointerType === "mouse" && event.button !== 0 || mouseMoving.current) {
            return;
        }
        onClick({ x, y });
    }, [onClick]);

    const texture = useMemo(
        () => Texture.fromBuffer(backgroundGraphic, backgroundGraphicSize, backgroundGraphicSize),
        [backgroundGraphic],
    );

    return (
        <Filters
            glow={{
                enabled: highlight,
                outerStrength: 5,
                innerStrength: 2,
                color: 0xfde90d,
                alpha: 1,
                knockout: false,
            }}
        >
            <Sprite
                position={{ x: x, y: y }}
                image="test"
                width={width}
                height={height}
                cullable={true}
                texture={texture}
                eventMode="static"
                mousedown={handleEvent}
                touchmove={mouseMoved}
                mousemove={mouseMoved}
                tap={handleEvent}
            />
        </Filters>
    );
};

export default BackgroundRectangle;
