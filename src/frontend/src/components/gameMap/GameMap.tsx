import { FC, useCallback, useEffect, useMemo, useRef } from "react";
import { Container, Stage } from "@pixi/react";
import Viewport from "./Viewport";
import { pixelColor } from "./Colors.ts";
import { mapConfig } from "./MapConfig";
import { getBackgroundGraphic, useNewMapStore } from "../../stores/newMapStore.ts";
import { useUser } from "../../hooks/useUser.ts";
import { EffectContainer, EffectContainerHandle } from "./particleEffects";
import { useOptimisticConquer } from "../../hooks/useOptimisticConquer.ts";
import { usePowerUpStore } from "../../stores/powerupStore.ts";
import { useIsMoving } from "../../hooks/useIsMoving.ts";
import { Coordinate } from "../../models/Coordinate.ts";
import { usePowerUps } from "../../hooks/usePowerUps.ts";
import MapTile from "./MapTile.tsx";
import { graphicsStore } from "../../stores/graphicsStore.ts";
/**
 * @component GameMap
 * ==================
 *
 * Wraps up all the processes for fetching data from API for map creation.
 * Processes the data in a quadtree data structure in order to present
 * a scalable and optimized map to guarantee a good experience for the
 * client.
 *
 * The map is rendered only if the client has a valid connection. Otherwise,
 * a span element indicates the current status.
 */
const GameMap: FC = () => {
    const pixelsBoundingBox = useNewMapStore((state) => state.pixelsBoundingBox);
    const map = useNewMapStore((state) => state.map);
    const { usePowerUp } = usePowerUps();
    const target = usePowerUpStore((state) => state.target);
    const { isMoving, startMoving } = useIsMoving();
    const effectRef = useRef<EffectContainerHandle>(null);
    const user = useUser();
    const conquer = useOptimisticConquer(user, effectRef);
    const onMapClickRef = useRef<((coordinate: Coordinate, targeted: boolean) => void) | null>(null);
    const { graphicsEnabled } = graphicsStore();

    const onMapClick = useCallback(
        (coordinate: Coordinate, targeted: boolean) => {
            const viewportX = coordinate.x / mapConfig.PixelSize;
            const viewportY = coordinate.y / mapConfig.PixelSize;
            const viewportCoordinate = { x: viewportX, y: viewportY };
            const powerUpClick = usePowerUp(viewportCoordinate, targeted);
            if (powerUpClick) return;
            conquer(viewportCoordinate);
        },
        [usePowerUp, conquer],
    );

    /**
     * onMapClick needs to be passed as a reference to the canvas elements in order to
     * avoid unnecessary renders caused by the changing function dependencies.
     * Without this, we will rerender every single element on the map every time any
     * tile changes, leading to huge performance problems.
     */
    useEffect(() => {
        onMapClickRef.current = onMapClick;
    }, [onMapClick, onMapClickRef]);

    const mappedBoundingBox = {
        minY: pixelsBoundingBox.min.y,
        minX: pixelsBoundingBox.min.x,
        maxY: pixelsBoundingBox.max.y,
        maxX: pixelsBoundingBox.max.x,
    };

    const pixelElements = useMemo(() => {
        const result: JSX.Element[] = [];
        if (map == null) return result;
        for (const [coordinate, pixel] of map) {
            const parsedCoordinate = JSON.parse(coordinate);
            const highlight = parsedCoordinate.x === target?.x || parsedCoordinate.y === target?.y;
            const rectangleX = parsedCoordinate.x * mapConfig.PixelSize;
            const rectangleY = parsedCoordinate.y * mapConfig.PixelSize;
            const color = pixelColor(pixel, user);

            let backgroundGraphic = undefined;
            if (graphicsEnabled) {
                backgroundGraphic = getBackgroundGraphic(coordinate);
            }

            result.push(
                <MapTile
                    key={`map-tile-${coordinate}`}
                    x={rectangleX}
                    y={rectangleY}
                    width={mapConfig.PixelSize}
                    height={mapConfig.PixelSize}
                    backgroundGraphic={backgroundGraphic}
                    hue={color.hue}
                    saturation={color.saturation}
                    lightness={color.lightness}
                    alpha={color.alpha}
                    highlight={highlight}
                    moving={isMoving}
                    onClickRef={onMapClickRef}
                />,
            );
        }
        return result;
    }, [map, target, isMoving, onMapClick]);

    if (map === null) {
        return <span>Loading...</span>;
    }
    return (
        <Stage
            width={window.innerWidth}
            height={window.innerHeight}
            options={{ background: 0xffffff, resizeTo: window, antialias: false, premultipliedAlpha: false }}
            onContextMenu={(e) => e.preventDefault()}
        >
            <Viewport
                width={window.innerWidth}
                height={window.innerHeight}
                boundingBox={mappedBoundingBox}
                onMoveStart={startMoving}
            >
                <Container>{pixelElements}</Container>
                <EffectContainer ref={effectRef} />
            </Viewport>
        </Stage>
    );
};

export default GameMap;
