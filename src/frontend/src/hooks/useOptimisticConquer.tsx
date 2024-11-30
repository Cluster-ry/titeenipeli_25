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

type Props = {
    user: User | null;
    effectHandle: RefObject<EffectContainerHandle>;
};

export const useOptimisticConquer = ({ user, effectHandle }: Props) => {
    const decreaseBucket = useGameStateStore((state) => state.decreaseBucket);
    const increaseBucket = useGameStateStore((state) => state.increaseBucket);
    const setPixel = useNewMapStore((state) => state.setPixel);

    const bucket = useGameStateStore((state) => state.pixelBucket);
    const map = useNewMapStore((state) => state.map);

    /**
     * @summary
     * onMutate return value is included in the "context" parameter,
     * allowing for easy rerolling of optimistic changes
     */
    const onConquerSettled = useCallback(
        async (success?: boolean, error?: Error | null, variables?: PostPixelsInput, context?: Pixel) => {
            if ((!success || error) && variables && context) {
                setPixel({ x: variables.x, y: variables.y }, context);
                effectHandle.current?.forbiddenEffect(variables);
                increaseBucket();
            }
        },
        [increaseBucket],
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
        [decreaseBucket, setPixel, user?.guild, user?.id],
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
        async (coordinate: Coordinate) => {
            const pixel = map?.get(JSON.stringify(coordinate));
            if (bucket.amount <= 0 || !pixel || pixel.guild == user?.guild || pixel.type != PixelType.Normal) {
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
                console.log(adjacentPixel);
                if (adjacentPixel && adjacentPixel.guild == user?.guild) {
                    conquerPixel(coordinate);
                    return;
                }
            }
        },
        [bucket.amount, map, user?.guild, conquerPixel],
    );

    return conquer;
};
