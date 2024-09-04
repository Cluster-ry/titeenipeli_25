import { create } from "zustand";
import { PlayerCoordinates } from "../models/PlayerCoordinates";
import { Guild } from "../components/gameMap/guild/Guild";
import { PixelMap } from "../models/PixelMap";
import { getPixels } from "../api/map";
import { GetPixelsResult } from "../models/Get/GetPixelsResult";
import { AxiosResponse } from "axios";
import { ViewportBoundigBox } from "../components/gameMap/Viewport";
import { getGrpcClient } from "../core/grpc/grpcClient";
import { IncrementalMapUpdateResponse } from "../generated/grpc/services/MapUpdate";

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
  mapSet: boolean;
  connectionStatus: ConnectionStatus;

  initializeMap: () => void;
  setPixels: (pixelMap: PixelMap) => void;
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
  mapSet: false,
  connectionStatus: ConnectionStatus.Disconnected,

  initializeMap: async () => {
    if (get().connectionStatus !== ConnectionStatus.Disconnected) {
      return;
    }

    set((state) => ({
      ...state,
      connectionStatus: ConnectionStatus.Connecting,
    }));
    const grpcClient = getGrpcClient();
    grpcClient.incrementalMapUpdateClient?.registerOnResponseListener(
      get().doIncrementalUpdate
    );
    // TODO: Hack, proper error handling should be done.
    const getPixelsResults =
      (await getPixels()) as AxiosResponse<GetPixelsResult>;
    const pixels = mapMatrixToMapDictionary(getPixelsResults.data);
    const pixelsBoundingBox = computeBoundingBox(pixels);
    set((state) => ({
      ...state,
      connectionStatus: ConnectionStatus.Connected,
      pixelsBoundingBox,
      pixels: pixels,
    }));
  },

  setConnectionStatus: (connectionStatus: ConnectionStatus) =>
    set((state) => ({ ...state, connectionStatus: connectionStatus })),

  setPixels: (pixelMap: PixelMap) =>
    set((state) => ({ ...state, pixels: pixelMap })),

  doIncrementalUpdate: (
    incrementalUpdateResponse: IncrementalMapUpdateResponse
  ) => {
    const pixels = get().pixels;
    for (const update of incrementalUpdateResponse.updates) {
      pixels.set(
        {
          x: update.spawnRelativeCoordinate?.x ?? 0,
          y: update.spawnRelativeCoordinate?.y ?? 0,
        },
        {
          type: "normal",
          owner: update.owner === 0 ? undefined: update.owner - 1,
          ownPixel: update.ownPixel,
        }
      );
    }
    set((state) => ({ ...state, pixels: pixels }))
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
      pixels.set({ x: x - playerX, y: y - playerY }, pixel);
    });
  });
  return pixels;
};

const computeBoundingBox = (pixels: PixelMap): ViewportBoundigBox => {
  let minX = Number.MAX_SAFE_INTEGER;
  let minY = Number.MAX_SAFE_INTEGER;
  let maxX = Number.MIN_SAFE_INTEGER;
  let maxY = Number.MIN_SAFE_INTEGER;
  for (const [coordinate, _] of pixels) {
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

export default useGameMapStore;
