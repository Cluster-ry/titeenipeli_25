import { RefObject, useCallback, useRef } from "react";
import { useGameStateStore } from "../stores/gameStateStore";
import { PostPixelsInput } from "../models/Post/PostPixelsInput";
import { Pixel } from "../models/Pixel";
import { useNewMapStore } from "../stores/newMapStore";
import { Coordinate } from "../models/Coordinate";
import PixelType from "../models/enum/PixelType";
import { User } from "../models/User";
import { useMapUpdating } from "./useMapUpdating";
import { EffectContainerHandle } from "../components/gameMap/particleEffects";

/**
 * @summary
 * Actions contain optimistically set next- and previous
 * values, making reverting bad changes possible
 */
type Action = {
    next: Pixel;
    previous: Pixel;
};

/**
 * @summary
 * Conquer a pixel with client-side optimistic handling
 * @param user
 * Current user
 * @param effectHandle
 * Reference to the effect handler
 * @returns
 * A function to commence a pixel conquest action
 */
export const useOptimisticConquer = (user: User | null, effectHandle: RefObject<EffectContainerHandle>) => {
    const { increaseBucket, decreaseBucket } = useGameStateStore();
    const { map, setPixel } = useNewMapStore();
    const pendingActions = useRef<{ [key: string]: Action[] }>({});

    const pushToActions = (key: string, next: Pixel, previous: Pixel) => {
        const actions = pendingActions.current[key] ?? [];
        actions.push({ next, previous });
        pendingActions.current[key] = actions;
    };

    const shiftActions = (key: string): Action | undefined => {
        const actions = pendingActions.current[key];
        if (!actions) return;
        const action = actions.shift();
        pendingActions.current[key] = actions;
        return action;
    };

    const clearActions = (key: string) => {
        const actions = pendingActions.current[key];
        if (!actions) return;
        pendingActions.current[key] = [];
    };

    /**
     * @summary
     * Is to be run when receiving a response from the server.
     * In case of an invalid optimistic placement, we roll back
     * the change with our cached previous value
     */
    const onConquerSettled = useCallback(
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        (success?: boolean, error?: Error | null, variables?: PostPixelsInput, _context?: Pixel) => {
            const action = variables ? shiftActions(JSON.stringify(variables)) : null;
            if ((!success || error) && variables) {
                increaseBucket();
                action?.previous && setPixel(variables, action.previous);
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
            const key = JSON.stringify(coordinate);
            const oldPixel = map?.get(key);
            effectHandle.current?.conqueredEffect(coordinate);
            decreaseBucket();
            const pixel = { guild: user?.guild, type: PixelType.Normal, owner: user?.id };
            setPixel(coordinate, pixel);
            pushToActions(key, pixel, oldPixel ?? pixel);
            return oldPixel;
        },
        [decreaseBucket, setPixel, effectHandle, user, map],
    );
    /**
     * @summary
     * Ran when map is updated via gRPC.
     * Overrides optimistic pixel placements and clears the cache
     * regarding these, since gRPC always returns the most current
     * map state. In case an optimistic placement is being processed
     * on the backend, it will still be sent by the gRPC.
     */
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const onPixelUpdated = useCallback((coordinate: Coordinate, _pixel: Pixel) => {
        const key = JSON.stringify(coordinate);
        clearActions(key);
    }, []);

    const { conquerPixel } = useMapUpdating({ optimisticConquer, onConquerSettled, events: { onPixelUpdated } });

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
            effectHandle.current?.forbiddenEffect(coordinate);
        },
        [map, user, effectHandle, conquerPixel],
    );

    return conquer;
};
