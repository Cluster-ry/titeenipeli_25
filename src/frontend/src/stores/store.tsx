import {
  instanceOfClientApiError,
  type ClientApiError,
} from "../models/ClientApiError";
import { create } from "zustand";
import { PlayerCoordinates } from "../models/PlayerCoordinates";
import { PixelMap } from "../models/PixelMap";
import { getPixels } from "../api/map";
import { GetPixelsResult } from "../models/Get/GetPixelsResult";
import { AxiosResponse } from "axios";
import { ViewportBoundigBox } from "../components/gameMap/Viewport";
import { getGrpcClient } from "../core/grpc/grpcClient";
import { IncrementalMapUpdateResponse } from "../generated/grpc/services/MapUpdate";
import PixelType from "../models/enum/PixelType";
import withRetry from "../utils/retryUtils";
import Guild from "../models/enum/Guild";

// The amount of rows and columns in the map. These can be
// changed to alter the map size.

export enum ConnectionStatus {
  Disconnected,
  Connecting,
  Connected,
}

interface GameMap {
  playerSpawn: PlayerCoordinates;
  playerGuild: Guild;
  pixels: PixelMap;
  pixelsBoundingBox: ViewportBoundigBox;
  initialized: boolean;
  connectionStatus: ConnectionStatus;
  incrementalUpdateBuffer: Array<IncrementalMapUpdateResponse>;

  initializeMap: () => void;
  reconnect: () => void;
  setPixels: (pixelMap: PixelMap) => void;
  handleGrpcConnectionStatusChanges: (connected: boolean) => void;
  doIncrementalUpdate: (
    incrementalUpdateResponse: IncrementalMapUpdateResponse
  ) => void;
  setPlayerGuild: (guild: number) => void;
}

export const useGameMapStore = create<GameMap>((set, get) => ({
  playerSpawn: { x: 0, y: 0 }, // Subject to change
  playerGuild: 2, // Cluster as default for TESTING
  pixelsBoundingBox: { minX: 0, minY: 0, maxX: 0, maxY: 0 },
  pixels: new Map(),
  initialized: false,
  connectionStatus: ConnectionStatus.Disconnected,
  incrementalUpdateBuffer: [],

  initializeMap: async () => {
    if (get().initialized === true) {
      return;
    }
    set((state) => ({
      ...state,
      initialized: true,
    }));

    const grpcClient = getGrpcClient();
    grpcClient.incrementalMapUpdateClient?.registerOnResponseListener(
      get().doIncrementalUpdate
    );
    grpcClient.registerOnConnectionStatusChangedListener(
      get().handleGrpcConnectionStatusChanges
    );
  },

  reconnect: async () => {
    if (get().connectionStatus !== ConnectionStatus.Disconnected) {
      return;
    }

    set((state) => ({
      ...state,
      connectionStatus: ConnectionStatus.Connecting,
    }));

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

  setPixels: (pixelMap: PixelMap) =>
    set((state) => ({ ...state, pixels: pixelMap })),

  handleGrpcConnectionStatusChanges: (connected: boolean) => {
    set((state) => ({
      ...state,
      connectionStatus: ConnectionStatus.Disconnected,
    }));

    if (connected) {
      get().reconnect();
    }
  },

  doIncrementalUpdate: (
    incrementalUpdateResponse: IncrementalMapUpdateResponse
  ) => {
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

  setPlayerGuild: (guild: number) =>
    set((state) => ({ ...state, playerGuild: guild })),
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

const parseIncrementalUpdate = (
  pixels: PixelMap,
  incrementalUpdateResponse: IncrementalMapUpdateResponse
) => {
  for (const update of incrementalUpdateResponse.updates) {
    pixels.set(
      JSON.stringify({
        x: update.spawnRelativeCoordinate?.x ?? 0,
        y: update.spawnRelativeCoordinate?.y ?? 0,
      }),
      {
        type: update.type as unknown as PixelType,
        owner: update.owner === 0 ? undefined : update.owner - 1,
        ownPixel: update.ownPixel,
      }
    );
  }
};

export default useGameMapStore;
