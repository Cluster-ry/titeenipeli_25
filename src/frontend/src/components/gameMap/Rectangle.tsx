import { Container, Sprite, withFilters } from "@pixi/react";
import { Color, FederatedPointerEvent, Texture } from "pixi.js";
import RectangleProps from "../../models/RectangleProps";
import { ColorOverlayFilter, GlowFilter } from "pixi-filters";
import { memo, useCallback, useMemo } from "react";

const backgroundGraphicSize = 32;

const Filters = withFilters(Container, {
    glow: GlowFilter,
    overlay: ColorOverlayFilter,
});

const BackgroundRectangle = (props: RectangleProps) => {
    const { x, y, width, height, backgroundGraphic, hue, saturation, lightness, alpha, highlight, moving, onClickRef } =
        props;
    const handleEvent = useCallback(
        (event: FederatedPointerEvent) => {
            if ((event.pointerType === "mouse" && event.button !== 0) || moving.current) {
                return;
            }
            const coordinate = { x, y };
            onClickRef.current && onClickRef.current(coordinate, highlight);
        },
        [x, y, highlight, moving, onClickRef],
    );

    const texture = useMemo(() => {
        if (backgroundGraphic !== undefined) {
            return Texture.fromBuffer(backgroundGraphic, backgroundGraphicSize, backgroundGraphicSize);
        } else {
            return Texture.WHITE;
        }
    }, [backgroundGraphic]);

    const overlayColor = useMemo(() => {
        if (hue !== undefined && saturation !== undefined && lightness !== undefined) {
            const pixiColor = new Color(`hsl(${hue}, ${saturation}%, ${lightness}%)`);
            return [pixiColor.red, pixiColor.green, pixiColor.blue];
        } else {
            return [0, 0, 0];
        }
    }, [hue, saturation, lightness]);
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
                    enabled: hue !== undefined,
                    color: overlayColor,
                    alpha: alpha ?? 0.75,
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
        [x, y, width, height, texture, highlight, overlayColor, handleEvent],
    );

    return sprite;
};

export default memo(BackgroundRectangle);
