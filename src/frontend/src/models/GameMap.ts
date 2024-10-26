import { PlayerCoordinates } from "./PlayerCoordinates";
import Guild from "./enum/Guild";
import { PixelMap } from "./PixelMap";
import { ViewportBoundigBox } from "../components/gameMap/Viewport";
import ConnectionStatus from "./enum/ConnectionStatus";
import { IncrementalMapUpdateResponse } from "../generated/grpc/services/MapUpdate";

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

export default GameMap;
