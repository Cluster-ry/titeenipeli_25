/**
 * @StateManagement
 * ================
 * A store file that provides Zustand state management for the client-side
 * of the titeenipelit 2025 project.
 *
 * The state management within this file concentrates in the map displayed
 * to the client.
 */

// From installed dependencies
import { create } from "zustand";
import { AxiosResponse } from "axios";

// Model imports
import { PixelMap } from "../models/PixelMap";
import PixelType from "../models/enum/PixelType";
import ConnectionStatus from "../models/enum/ConnectionStatus";
import { GetPixelsResult } from "../models/Get/GetPixelsResult";
import { instanceOfClientApiError, type ClientApiError } from "../models/ClientApiError";
import GameMap from "../models/GameMap";
import { getPixels } from "../api/map";
import ViewportBoundigBox from "../models/ViewportBoundigBox";
import { GrpcClient } from "../core/grpc/grpcClient";
import { IncrementalMapUpdateResponse } from "../generated/grpc/services/StateUpdate";
import withRetry from "../utils/retryUtils";

export const useGameMapStore = create<GameMap>((set, get) => ({
    playerSpawn: { x: 0, y: 0 }, // Subject to change
    playerGuild: 2, // Cluster as default for TESTING
    pixelsBoundingBox: { minX: 0, minY: 0, maxX: 0, maxY: 0 },
    pixels: new Map(),
    initialized: false,
    connectionStatus: ConnectionStatus.Disconnected,
    incrementalUpdateBuffer: [],

    /**
     * Initializes the map. Makes sure it has not been done already and
     * calls the GRPC client.
     */
    initializeMap: async () => {
        // Returning if the map has already been initialized
        if (get().initialized === true) {
            return;
        }

        // Setting the status to initialized
        set((state) => ({
            ...state,
            initialized: true,
        }));

        // Calling the GRPC client

        const grpcClient = GrpcClient.getGrpcClient();
        grpcClient.incrementalMapUpdateClient?.registerOnResponseListener(get().doIncrementalUpdate);
        grpcClient.registerOnConnectionStatusChangedListener(get().handleGrpcConnectionStatusChanges);
    },

    /**
     * Attempts to reconnect the user if their current status is
     * disconnected.
     */
    reconnect: async () => {
        if (get().connectionStatus !== ConnectionStatus.Disconnected) {
            return;
        }

        // Setting the status to "connecting"
        set((state) => ({
            ...state,
            connectionStatus: ConnectionStatus.Connecting,
        }));

        // Updating the current state of the map for the user
        const getPixelsResults = await withRetry(async () => {
            const getPixelsResults = await getPixels();
            if (instanceOfClientApiError(getPixelsResults)) {
                const error = getPixelsResults as ClientApiError;
                throw new Error(error.msg);
            }
            return getPixelsResults as AxiosResponse<GetPixelsResult>;
        });

        const pixels = mapMatrixToMapDictionary(getPixelsResults.data);
        const pixelsBoundingBox = computeBoundingBox(pixels);

        // With the map updated, the user has successfully reconnected
        set((state) => ({
            ...state,
            connectionStatus: ConnectionStatus.Connected,
            pixelsBoundingBox,
            pixels: pixels,
        }));

        get().doIncrementalUpdate({ updates: [] });
    },

    setConnectionStatus: (connectionStatus: ConnectionStatus) =>
        set((state) => ({ ...state, connectionStatus: connectionStatus })),

    setPixels: (pixelMap: PixelMap) => set((state) => ({ ...state, pixels: pixelMap })),

    /**
     * Sets the connection status to "disconnected", but attempts to reconnect
     * if given
     */
    handleGrpcConnectionStatusChanges: (connected: boolean) => {
        set((state) => ({
            ...state,
            connectionStatus: ConnectionStatus.Disconnected,
        }));

        if (connected) {
            get().reconnect();
        }
    },

    doIncrementalUpdate: (incrementalUpdateResponse: IncrementalMapUpdateResponse) => {
        const incrementalUpdateBuffer = get().incrementalUpdateBuffer;
        incrementalUpdateBuffer.push(incrementalUpdateResponse);

        if (get().connectionStatus === ConnectionStatus.Connected) {
            for (const incrementalUpdate of incrementalUpdateBuffer) {
                const pixels = get().pixels;
                parseIncrementalUpdate(pixels, incrementalUpdate);

                const pixelsBoundingBox = computeBoundingBox(pixels);
                set((state) => ({
                    ...state,
                    pixelsBoundingBox,
                    pixels,
                    incrementalUpdateBuffer: [],
                }));
            }
        } else {
            set((state) => ({ ...state, incrementalUpdateBuffer }));
        }
    },

    setPlayerGuild: (guild: number) => set((state) => ({ ...state, playerGuild: guild })),
}));

const mapMatrixToMapDictionary = (results: GetPixelsResult) => {
    const pixels: PixelMap = new Map();
    const playerX = results.playerSpawn.x;
    const playerY = results.playerSpawn.y;

    results.pixels.map((layer, y) => {
        layer.map((pixel, x) => {
            pixels.set(JSON.stringify({ x: x - playerX, y: y - playerY }), pixel);
        });
    });
    return pixels;
};

const computeBoundingBox = (pixels: PixelMap): ViewportBoundigBox => {
    let minX = Number.MAX_SAFE_INTEGER;
    let minY = Number.MAX_SAFE_INTEGER;
    let maxX = Number.MIN_SAFE_INTEGER;
    let maxY = Number.MIN_SAFE_INTEGER;
    for (const [serializedCoordinate] of pixels) {
        const coordinate = JSON.parse(serializedCoordinate);
        if (coordinate.x < minX) minX = coordinate.x;
        if (coordinate.y < minY) minY = coordinate.y;
        if (coordinate.x > maxX) maxX = coordinate.x;
        if (coordinate.y > maxY) maxY = coordinate.y;
    }
    return {
        minX,
        minY,
        maxX,
        maxY,
    };
};

const parseIncrementalUpdate = (pixels: PixelMap, incrementalUpdateResponse: IncrementalMapUpdateResponse) => {
    for (const update of incrementalUpdateResponse.updates) {
        const pixelType = update.type as unknown as PixelType;
        const pixelKey = JSON.stringify({
            x: update.spawnRelativeCoordinate?.x ?? 0,
            y: update.spawnRelativeCoordinate?.y ?? 0,
        });

        if (pixelType !== PixelType.FogOfWar) {
            pixels.set(pixelKey, {
                type: pixelType,
                owner: update.owner === 0 ? undefined : update.owner - 1,
                ownPixel: update.ownPixel,
            });
        } else {
            pixels.delete(pixelKey);
        }
    }
};

export default useGameMapStore;
