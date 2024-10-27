import { Coordinate, useNewMapStore } from "../stores/newMapStore.ts";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { getPixels } from "../api/map.ts";
import { useCallback, useEffect } from "react";
import { IncrementalMapUpdateResponse } from "../generated/grpc/services/StateUpdate.ts";
import PixelType from "../models/enum/PixelType.ts";
import { GetPixelsResult } from "../models/Get/GetPixelsResult.ts";
import { Pixel } from "../models/Pixel.ts";

export const useMapUpdating = () => {
    const mapQueryKey = "map";
    const incrementalUpdateBuffer: IncrementalMapUpdateResponse[] = [];
    const queryClient = useQueryClient();
    const { map, setMap, setPixel, setPixelsBoundingBox } = useNewMapStore();
    const consumeUpdate = useCallback((update: IncrementalMapUpdateResponse) => {
        for (const updatedPixel of update.updates) {
            const pixelType = updatedPixel.type as unknown as PixelType;
            const pixelCoordinates = {
                x: updatedPixel.spawnRelativeCoordinate?.x ?? 0,
                y: updatedPixel.spawnRelativeCoordinate?.y ?? 0,
            };
            if (pixelType !== PixelType.FogOfWar) {
                setPixel(pixelCoordinates, {
                    type: pixelType,
                    guild: updatedPixel.guild,
                    owner: updatedPixel.owner?.id,
                    ...pixelCoordinates,
                });
            } else {
                setPixel(pixelCoordinates, null);
            }
        }
    }, []);
    const consumeUpdates = useCallback(() => {
        while (incrementalUpdateBuffer.length > 0) {
            // We can use a bang to simplify type checker's work because of the length condition for the while loop
            const oldestUpdate = incrementalUpdateBuffer.shift()!;
            consumeUpdate(oldestUpdate);
        }
    }, []);
    const { data, isSuccess } = useQuery({
        queryKey: [mapQueryKey],
        queryFn: getPixels,
        refetchInterval: false,
    });

    const mapMatrixToMapDictionary = useCallback((results: GetPixelsResult) => {
        const pixels: Map<Coordinate, Pixel> = new Map();
        const playerX = results.playerSpawn.x;
        const playerY = results.playerSpawn.y;

        results.pixels.map((layer, y) => {
            layer.map((pixel, x) => {
                if (pixel.type !== PixelType.FogOfWar) pixels.set({ x: x - playerX, y: y - playerY }, pixel);
            });
        });
        return pixels;
    }, []);

    const computeBoundingBox = useCallback((pixels: Map<Coordinate, Pixel>) => {
        let minX = Number.MAX_SAFE_INTEGER;
        let minY = Number.MAX_SAFE_INTEGER;
        let maxX = Number.MIN_SAFE_INTEGER;
        let maxY = Number.MIN_SAFE_INTEGER;
        for (const [pixel] of pixels) {
            if (pixel.x < minX) minX = pixel.x;
            if (pixel.y < minY) minY = pixel.y;
            if (pixel.x > maxX) maxX = pixel.x;
            if (pixel.y > maxY) maxY = pixel.y;
        }
        return {
            min: {
                x: minX,
                y: minY,
            },
            max: {
                x: maxX,
                y: maxY,
            },
        };
    }, []);

    const onIncrementalUpdate = useCallback(
        (incrementalUpdateResponse: IncrementalMapUpdateResponse) => {
            if (!isSuccess || incrementalUpdateBuffer.length > 0) {
                incrementalUpdateBuffer.push(incrementalUpdateResponse);
                // Technically this else-branch is not fully parallel-safe. Return here if race conditions occur!
            } else {
                consumeUpdate(incrementalUpdateResponse);
            }
        },
        [incrementalUpdateBuffer],
    );

    const ensureMap = useCallback(async () => {
        setMap(null);
        await queryClient.invalidateQueries({ queryKey: [mapQueryKey] });
    }, []);

    useEffect(() => {
        if (isSuccess) {
            if (map === null) {
                const mappedPixels = mapMatrixToMapDictionary(data.data);
                setMap(mappedPixels);
                setPixelsBoundingBox(computeBoundingBox(mappedPixels));
            }
            consumeUpdates();
        }
    }, [isSuccess]);

    return { onIncrementalUpdate, ensureMap };
};
