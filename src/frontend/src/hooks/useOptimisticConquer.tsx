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
     * Is to be run when receiving a response from the server.
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

    /**
     * @summary
     * Is to be run before sending a request to the server.
     * Displays the visual of the users' actions before receiving
     * a definitive response from the server.
     */
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
     * @summary
     * Executes when the client attempts to conquer a pixel for their guild.
     * Changes the integer value representing a guild to the one associated
     * with the client's own guild.
     * 
     * Do basic condition checks before sending the update. In case the basic
     * pixel placement rules are not met, display error effect and forfeit from
     * making the request.
     * 
     * Rules:
     * - At least one of the adjacent pixels must be owned by the guild
     * - The targeted pixel must be a conquerable type (Normal)
     * - There must be enough buffer left in the pixelBucket to make the action
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
