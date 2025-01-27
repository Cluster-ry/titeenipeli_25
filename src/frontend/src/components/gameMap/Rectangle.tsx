import { Container, Sprite, withFilters } from "@pixi/react";
import { Color, FederatedPointerEvent, Texture } from "pixi.js";
import RectangleProps from "../../models/RectangleProps";
import { ColorOverlayFilter, GlowFilter } from "pixi-filters";
import { useCallback, useMemo } from "react";

const backgroundGraphicSize = 32;

const Filters = withFilters(Container, {
    glow: GlowFilter,
    overlay: ColorOverlayFilter,
});

const BackgroundRectangle = ({
    x,
    y,
    width,
    height,
    backgroundGraphic,
    color,
    highlight,
    moving,
    onClick,
}: RectangleProps) => {
    const handleEvent = useCallback(
        (event: FederatedPointerEvent) => {
            if ((event.pointerType === "mouse" && event.button !== 0) || moving.current) {
                return;
            }
            onClick({ x, y });
        },
        [onClick],
    );

    const texture = useMemo(() => {
        if (backgroundGraphic !== undefined) {
            return Texture.fromBuffer(backgroundGraphic, backgroundGraphicSize, backgroundGraphicSize);
        } else {
            return Texture.WHITE;
        }
    }, [backgroundGraphic]);

    const overlayColor = useMemo(() => {
        if (color) {
            const pixiColor = new Color(`hsl(${color.hue}, ${color.saturation}%, ${color.lightness}%)`);
            return [pixiColor.red, pixiColor.green, pixiColor.blue];
        } else {
            return [0, 0, 0];
        }
    }, [color]);

    const sprite = useMemo(
        () => (
            <Filters
                glow={{
                    enabled: highlight,
                    outerStrength: 5,
                    innerStrength: 2,
                    color: 0xfde90d,
                    alpha: 1,
                    knockout: false,
                }}
                overlay={{
                    enabled: color !== undefined,
                    color: overlayColor,
                    alpha: color?.alpha ?? 0.75,
                }}
            >
                <Sprite
                    position={{ x, y }}
                    width={width}
                    height={height}
                    cullable={false}
                    texture={texture}
                    eventMode="static"
                    mousedown={handleEvent}
                    tap={handleEvent}
                />
            </Filters>
        ),
        [x, y, width, height, texture, handleEvent],
    );

    return sprite;
};

export default BackgroundRectangle;
