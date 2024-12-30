import { FC, useCallback, useMemo, useRef } from "react";
import { Container, Stage } from "@pixi/react";
import Viewport from "./Viewport";
import Rectangle from "./Rectangle";
import { pixelColor } from "./guild/Guild";
import { mapConfig } from "./MapConfig";
import { Coordinate } from "../../models/Coordinate";
import { postPixels } from "../../api/map";
import PixelType from "../../models/enum/PixelType.ts";
import { useNewMapStore } from "../../stores/newMapStore.ts";
import { useMapUpdating } from "../../hooks/useMapUpdating.ts";
import { useUser } from "../../hooks/useUser.ts";
import { EffectContainer, EffectContainerHandle } from "./particleEffects";
import { useGameStateStore } from "../../stores/gameStateStore.ts";
import { User } from "../../models/User.ts";

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
    const bucket = useGameStateStore((state) => state.pixelBucket);
    const decreaseBucket = useGameStateStore((state) => state.decreaseBucket);
    const effectRef = useRef<EffectContainerHandle>(null);
    useMapUpdating();
    const user: User | null = useUser();

    /**
     * Executes when the client conquers a pixel for their guild.
     * Changes the integer value representing a guild to the one
     * associated with the client's own guild.
     *
     * @note Upon change, the map is automatically refreshed.
     */
    const conquer = useCallback(
        async (coordinate: Coordinate) => {
            if (bucket.amount <= 0) {
                effectRef.current?.forbiddenEffect(coordinate);
                return;
            }

            const success = await postPixels(coordinate);
            if (success) {
                effectRef.current?.conqueredEffect(coordinate);
                decreaseBucket();
            } else {
                effectRef.current?.forbiddenEffect(coordinate);
            }
        },
        [bucket.amount, decreaseBucket],
    );

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
            const rectangleX = coordinate.x * mapConfig.PixelSize;
            const rectangleY = coordinate.y * mapConfig.PixelSize;
            const color = pixelColor(pixel, user);
            result.push(
                <Rectangle
                    key={`x:${coordinate.x} y:${coordinate.y}-${result.length}-${Date.now()}`}
                    x={rectangleX}
                    y={rectangleY}
                    isOwn={pixel?.owner === user?.id}
                    isSpawn={pixel?.type === PixelType.Spawn}
                    width={mapConfig.PixelSize}
                    height={mapConfig.PixelSize}
                    color={color}
                    onClick={() => conquer(coordinate)}
                />,
            );
        }
        return result;
    }, [map]);

    if (map === null) {
        return <span>Loading...</span>;
    }

    return (
        <>
            <Stage
                width={window.innerWidth}
                height={window.innerHeight}
                options={{ background: 0xffffff, resizeTo: window }}
            >
                <Viewport width={window.innerWidth} height={window.innerHeight} boundingBox={mappedBoundingBox}>
                    <Container>{pixelElements}</Container>
                    <EffectContainer ref={effectRef} />
                </Viewport>
            </Stage>
        </>
    );
};

export default GameMap;
