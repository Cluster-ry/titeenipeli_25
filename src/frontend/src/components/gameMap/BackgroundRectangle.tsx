import { Container, Sprite, withFilters } from "@pixi/react";
import { FederatedPointerEvent, Texture } from "pixi.js";
import BackgroundRectangleProps from "../../models/BackgroundRectangleProps";
import { GlowFilter } from "pixi-filters";
import { ReactNode, useCallback, useMemo } from "react";

const backgroundGraphicSize = 32;

const Filters = withFilters(Container, {
    glow: GlowFilter,
});

const filterSprite = (child: ReactNode) => (
    <Filters
        glow={{
            outerStrength: 5,
            innerStrength: 2,
            color: 0xfde90d,
            alpha: 1,
            knockout: false,
        }}
    >
        {child}
    </Filters>
);

const BackgroundRectangle = ({
    x,
    y,
    width,
    height,
    backgroundGraphic,
    highlight,
    moving,
    onClick,
}: BackgroundRectangleProps) => {
    const handleEvent = useCallback(
        (event: FederatedPointerEvent) => {
            if (event.pointerType === "mouse" && event.button !== 0 || moving.current) {
                return;
            }
            onClick({ x, y });
        },
        [onClick],
    );

    const texture = useMemo(
        () => Texture.fromBuffer(backgroundGraphic, backgroundGraphicSize, backgroundGraphicSize),
        [backgroundGraphic],
    );

    const sprite = useMemo(() => (
        <Sprite
            position={{ x, y }}
            image="test"
            width={width}
            height={height}
            cullable={true}
            texture={texture}
            eventMode="static"
            mousedown={handleEvent}
            tap={handleEvent}
        />
    ), [x, y , width, height, texture, handleEvent]);

    return highlight ? filterSprite(sprite) : sprite;
};

export default BackgroundRectangle;
