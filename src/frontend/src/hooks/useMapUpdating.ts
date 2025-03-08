import { IncrementalMapUpdateResponse } from "./../generated/grpc/services/StateUpdate";
import {
    deleteBackgroundGraphic,
    setBackgroundGraphic,
    updateBackgroundGraphic,
    useNewMapStore,
} from "../stores/newMapStore.ts";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getPixels, postPixels } from "../api/map.ts";
import { useCallback, useEffect, useRef, useState } from "react";
import PixelType from "../models/enum/PixelType.ts";
import { GetPixelsResult } from "../models/Get/GetPixelsResult.ts";
import { EncodedPixel, Pixel } from "../models/Pixel.ts";
import GrpcClients from "../core/grpc/grpcClients.ts";
import { PostPixelsInput } from "../models/Post/PostPixelsInput.ts";
import { Coordinate } from "../models/Coordinate.ts";
import { getBoundaries } from "../utils/getBoundaries.ts";

type Events = {
    onPixelUpdated?: (coordinates: Coordinate, value: Pixel) => void;
};

const mapQueryKey = "map";
const postPixelKey = "pixel";

export const useMapUpdating = (
    optimisticConquer: (data: PostPixelsInput) => void,
    onConquerSettled: (data?: boolean, error?: Error | null, variables?: PostPixelsInput) => void,
    events: Events = {},
) => {
    const incrementalUpdateBuffer = useRef<IncrementalMapUpdateResponse[]>([]);
    const queryClient = useQueryClient();
    const grpcClient = useRef<GrpcClients>(GrpcClients.getGrpcClients());

    const [grpcConnected, setGrpcConnected] = useState(false);

    const setMap = useNewMapStore((state) => state.setMap);
    const setPixel = useNewMapStore((state) => state.setPixel);
    const setPixelsBoundingBox = useNewMapStore((state) => state.setPixelsBoundingBox);
    const updatePixelsBoundingBox = useNewMapStore((state) => state.updatePixelsBoundingBox);

    const getPixelsWithGrpcConnectionRequirement = () => {
        if (!grpcConnected) {
            throw new Error("gRPC connection is not ready yet!");
        }
        return getPixels();
    };

    const { data, isSuccess, status } = useQuery({
        queryKey: [mapQueryKey],
        queryFn: getPixelsWithGrpcConnectionRequirement,
        refetchOnReconnect: "always",
        refetchOnMount: "always",
        retry: true,
    });

    const { mutate: conquerPixel } = useMutation({
        mutationKey: [postPixelKey],
        mutationFn: postPixels,
        onMutate: optimisticConquer,
        onSettled: onConquerSettled,
    });

    const consumeUpdate = useCallback(
        (update: IncrementalMapUpdateResponse) => {
            const processedCoordinates: Coordinate[] = [];
            for (const updatedPixel of update.updates) {
                const pixelType = updatedPixel.type as unknown as PixelType;
                const pixelCoordinates = {
                    x: updatedPixel.spawnRelativeCoordinate?.x ?? 0,
                    y: updatedPixel.spawnRelativeCoordinate?.y ?? 0,
                };
                let pixel: (Coordinate & Pixel) | null = null;
                if (pixelType !== PixelType.FogOfWar) {
                    pixel = {
                        type: pixelType,
                        guild: updatedPixel.guild ? Number(updatedPixel.guild) : undefined,
                        owner: updatedPixel.owner?.id,
                        ...pixelCoordinates,
                    };
                    events.onPixelUpdated && events.onPixelUpdated(pixelCoordinates, pixel);
                    updateBackgroundGraphic(JSON.stringify(pixelCoordinates), updatedPixel.backgroundGraphic);
                } else {
                    deleteBackgroundGraphic(JSON.stringify(pixelCoordinates));
                }
                setPixel(pixelCoordinates, pixel);
                processedCoordinates.push(pixelCoordinates);
            }
            updatePixelsBoundingBox(processedCoordinates);
        },
        [setPixel, updatePixelsBoundingBox, events],
    );
    const consumeUpdates = useCallback(() => {
        while (incrementalUpdateBuffer.current.length > 0) {
            // We can use a bang to simplify type checker's work because of the length condition for the while loop
            const oldestUpdate = incrementalUpdateBuffer.current.shift()!;
            consumeUpdate(oldestUpdate);
        }
    }, [incrementalUpdateBuffer, consumeUpdate]);

    const decodePixel = useCallback((coordinate: string, encodedPixel: EncodedPixel) => {
        const decodedGraphics = encodedPixel.backgroundGraphic
            ? Uint8Array.from(atob(encodedPixel.backgroundGraphic), (c) => c.charCodeAt(0))
            : undefined;
        setBackgroundGraphic(coordinate, decodedGraphics);
        const decodePixel: Pixel = {
            type: encodedPixel.type,
            guild: encodedPixel.guild,
            owner: encodedPixel.owner,
        };
        return decodePixel;
    }, []);

    const mapMatrixToMapDictionary = useCallback(
        (results: GetPixelsResult) => {
            const pixels: Map<string, Pixel> = new Map();
            const playerX = results.playerSpawn.x;
            const playerY = results.playerSpawn.y;

            results.pixels.map((layer, y) => {
                layer.map((pixel, x) => {
                    if (pixel.type !== PixelType.FogOfWar) {
                        const coordinate = JSON.stringify({ x: x - playerX, y: y - playerY });
                        const decodedPixel = decodePixel(coordinate, pixel);
                        pixels.set(coordinate, decodedPixel);
                    }
                });
            });
            return pixels;
        },
        [decodePixel],
    );

    const computeBoundingBox = useCallback((pixels: Map<string, Pixel>) => {
        return getBoundaries(Array.from(pixels.keys()).map(x => JSON.parse(x)));
    }, []);

    const onIncrementalUpdate = useCallback(
        (incrementalUpdateResponse: IncrementalMapUpdateResponse) => {
            if (!isSuccess || incrementalUpdateBuffer.current.length > 0) {
                console.debug("Storing update, success:", isSuccess, status);
                console.debug("Update buffer:", incrementalUpdateBuffer);
                incrementalUpdateBuffer.current.push(incrementalUpdateResponse);
                // Technically this else-branch is not fully parallel-safe. Return here if race conditions occur!
            } else {
                console.debug("Consuming separate update:", incrementalUpdateResponse);
                consumeUpdate(incrementalUpdateResponse);
            }
        },
        [incrementalUpdateBuffer, isSuccess, status, consumeUpdate],
    );

    const resetMap = useCallback(async () => {
        setMap(null);
        await queryClient.resetQueries({ queryKey: [mapQueryKey] });
    }, [queryClient, setMap]);

    const onGrpcConnectionStatusChanged = useCallback(
        async (connected: boolean) => {
            setGrpcConnected(connected);
            if (connected === false) {
                await resetMap();
            }
        },
        [resetMap, setGrpcConnected],
    );

    const ensureMap = useCallback(async () => {
        await resetMap();
    }, [resetMap]);

    useEffect(() => {
        console.debug("Rendering useMapUpdating hook. Success:", isSuccess);
        if (isSuccess) {
            const mappedPixels = mapMatrixToMapDictionary(data.data);
            console.debug("Fetched map:", mappedPixels);
            setMap(mappedPixels);
            setPixelsBoundingBox(computeBoundingBox(mappedPixels));
            console.debug("Consuming updates as part of the rendering process");
            consumeUpdates();
        }
    }, [isSuccess, data?.data, setMap, setPixelsBoundingBox, consumeUpdates, computeBoundingBox]);

    useEffect(() => {
        console.debug("Registering GRPC client for incremental updates");
        grpcClient.current.incrementalMapUpdateClient.registerOnResponseListener(onIncrementalUpdate);
        return () => {
            grpcClient.current.incrementalMapUpdateClient.unRegisterOnResponseListener(onIncrementalUpdate);
        };
    }, [onIncrementalUpdate]);

    useEffect(() => {
        grpcClient.current.incrementalMapUpdateClient.registerOnConnectionStatusChangedListener(
            onGrpcConnectionStatusChanged,
        );
        return () => {
            grpcClient.current.incrementalMapUpdateClient.unRegisterOnConnectionStatusChangedListener(
                onGrpcConnectionStatusChanged,
            );
        };
    }, [onGrpcConnectionStatusChanged]);

    return { ensureMap, conquerPixel };
};
