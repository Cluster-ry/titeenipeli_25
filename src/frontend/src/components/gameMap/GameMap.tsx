import { FC, useCallback, useMemo, useRef } from "react";
import { Container, Stage } from "@pixi/react";
import Viewport from "./Viewport";
import ForegroundRectangle from "./ForegroundRectangle.tsx";
import { pixelColor } from "./Colors.ts";
import { mapConfig } from "./MapConfig";
import PixelType from "../../models/enum/PixelType.ts";
import { useNewMapStore } from "../../stores/newMapStore.ts";
import { useUser } from "../../hooks/useUser.ts";
import { EffectContainer, EffectContainerHandle } from "./particleEffects";
import { useOptimisticConquer } from "../../hooks/useOptimisticConquer.ts";
import { Coordinate } from "../../models/Coordinate.ts";
import BackgroundRectangle from "./BackgroundRectangle.tsx";
import { usePowerUps } from "../../hooks/usePowerUps.ts";
import { usePowerUpStore } from "../../stores/powerupStore.ts";
import { useIsMoving } from "../../hooks/useIsMoving.ts";

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
    const target = usePowerUpStore(state => state.target);
    const { isMoving, startMoving, stopMoving } = useIsMoving();
    const effectRef = useRef<EffectContainerHandle>(null);
    const user = useUser();
    const conquer = useOptimisticConquer(user, effectRef);

    const handleMapClick = useCallback((coordinate: Coordinate, targeted: boolean) => {
        const powerUpClick = usePowerUp(coordinate, targeted);
        if (powerUpClick) return;
        conquer(coordinate);
    }, [usePowerUp, conquer]);

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
            if (pixel && pixel.backgroundGraphic) {
                result.push(
                    <BackgroundRectangle
                        key={`background-${coordinate}-${result.length}-${Date.now()}`}
                        x={rectangleX}
                        y={rectangleY}
                        width={mapConfig.PixelSize}
                        height={mapConfig.PixelSize}
                        backgroundGraphic={pixel?.backgroundGraphic}
                        highlight={highlight}
                        moving={isMoving}
                        onClick={() => handleMapClick(parsedCoordinate, highlight)}
                    />,
                );
            }
            if (pixel?.owner || pixel?.guild || pixel?.type == PixelType.MapBorder) {
                result.push(
                    <ForegroundRectangle
                        key={`foreground-${coordinate}-${result.length}-${Date.now()}`}
                        x={rectangleX}
                        y={rectangleY}
                        width={mapConfig.PixelSize}
                        height={mapConfig.PixelSize}
                        color={color}
                    />,
                );
            }
        }
        return result;
    }, [map, target]);

    if (map === null) {
        return <span>Loading...</span>;
    }
    //console.log(isMoving);
    return (
            <Stage
                width={window.innerWidth}
                height={window.innerHeight}
                options={{ background: 0xffffff, resizeTo: window }}
                onContextMenu={(e) => e.preventDefault()}
            >
                <Viewport
                    width={window.innerWidth}
                    height={window.innerHeight}
                    boundingBox={mappedBoundingBox}
                    onMoveStart={startMoving}
                    onMoveEnd={stopMoving}
                >
                    <Container >{pixelElements}</Container>
                    <EffectContainer ref={effectRef} />
                </Viewport>
            </Stage>
    );
};

export default GameMap;
