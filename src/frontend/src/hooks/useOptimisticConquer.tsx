import { RefObject, useCallback } from "react";
import { useGameStateStore } from "../stores/gameStateStore";
import { PostPixelsInput } from "../models/Post/PostPixelsInput";
import { Pixel } from "../models/Pixel";
import { useNewMapStore } from "../stores/newMapStore";
import { Coordinate } from "../models/Coordinate";
import PixelType from "../models/enum/PixelType";
import { User } from "../models/User";
import { useMapUpdating } from "./useMapUpdating";
import { EffectContainerHandle } from "../components/gameMap/particleEffects";

export const useOptimisticConquer = (user: User | null, effectHandle: RefObject<EffectContainerHandle>) => {
    const { increaseBucket, decreaseBucket } = useGameStateStore();
    const { map, setPixel } = useNewMapStore();

    /**
     * @summary
     * onMutate return value is included in the "context" parameter,
     * allowing for easy rerolling of optimistic changes
     */
    const onConquerSettled = useCallback(
        (success?: boolean, error?: Error | null, variables?: PostPixelsInput, context?: Pixel) => {
            if ((!success || error) && variables && context) {
                increaseBucket();
                setPixel({ x: variables.x, y: variables.y }, context);
                effectHandle.current?.forbiddenEffect(variables);
            }
        },
        [increaseBucket, setPixel, effectHandle],
    );

    // Optimistic conquer effect
    const optimisticConquer = useCallback(
        (coordinate: PostPixelsInput) => {
            const oldPixel = map?.get(JSON.stringify(coordinate));
            effectHandle.current?.conqueredEffect(coordinate);
            decreaseBucket();
            setPixel(
                { x: coordinate.x, y: coordinate.y },
                { guild: user?.guild, type: PixelType.Normal, owner: user?.id },
            );
            return oldPixel;
        },
        [decreaseBucket, setPixel, effectHandle, user, map],
    );

    const { conquerPixel } = useMapUpdating({ optimisticConquer, onConquerSettled });

    /**
     * Executes when the client conquers a pixel for their guild.
     * Changes the integer value representing a guild to the one
     * associated with the client's own guild.
     *
     * @note Upon change, the map is automatically refreshed.
     */
    const conquer = useCallback(
        (coordinate: Coordinate) => {
            const pixel = map?.get(JSON.stringify(coordinate));
            // For some god knows what reason, pixelBucket won't trigger a rerender if we don't get the state manually when receiving an update through grpc
            if (
                useGameStateStore.getState().pixelBucket.amount <= 0 ||
                !pixel ||
                pixel.guild == user?.guild ||
                pixel.type != PixelType.Normal
            ) {
                effectHandle.current?.forbiddenEffect(coordinate);
                return;
            }
            // Check basic pixel placement rules to avoid unnecessary traffic on the server
            const { x, y } = coordinate;
            const adjacent = [
                [1, 0],
                [0, 1],
                [-1, 0],
                [0, -1],
            ];
            for (const value of adjacent) {
                const adjacentCoordinate = { x: x + value[0], y: y + value[1] };
                const adjacentPixel = map?.get(JSON.stringify(adjacentCoordinate));
                if (adjacentPixel && adjacentPixel.guild == user?.guild) {
                    conquerPixel(coordinate);
                    return;
                }
            }
        },
        [map, user, effectHandle, conquerPixel],
    );

    return conquer;
};
