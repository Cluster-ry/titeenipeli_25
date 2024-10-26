import {Container, Stage} from "@pixi/react";
import Viewport from "./Viewport";
import Rectangle from "./Rectangle";
import { useMemo } from "react";
import ConnectionStatus from "../../models/enum/ConnectionStatus";
import { pixelColor } from "./guild/Guild";
import { mapConfig } from "./MapConfig";
import { Coordinate } from "../../models/Coordinate";
import useGameMapStore from "../../stores/store";
import { postPixels } from "../../api/map";
import PixelType from "../../models/enum/PixelType.ts";

/**
 * @component GameMap
 * ==================
 *
 * Wraps up all the processes for fetching data from API for map creation.
 * Processes the data in a quadtree data structure in order to present
 * a scalable and optimized map to guarantee a good experience for the
 * client.
 *
 * The map is rendered only if the client has a valid connection. Otherwise,
 * a span element indicates the current status.
 */
const GameMap = () => {
  const gameMapStore = useGameMapStore((state) => state);
  useMemo(() => {
    gameMapStore.initializeMap();
  }, []);

  /**
   * Executes when the client conquers a pixel for their guild.
   * Changes the integer value representing a guild to the one
   * associated with the client's own guild.
   *
   * @note Upon change, the map is automatically refreshed.
   */
  async function conquer(coordinate: Coordinate) {
    await postPixels(coordinate);
  }
  
  // Handling undesired states. Returning if the client is not connected
  if (gameMapStore.connectionStatus === ConnectionStatus.Disconnected) {
    return <span>Disconnected</span>;
  } else if (gameMapStore.connectionStatus === ConnectionStatus.Connecting) {
    return <span>Loading...</span>;
  }


  const pixelElements = [];
  for (const [serializedCoordinate, pixel] of gameMapStore.pixels) {
    const coordinate = JSON.parse(serializedCoordinate);
    const rectangleX = coordinate.x * mapConfig.PixelSize;
    const rectangleY = coordinate.y * mapConfig.PixelSize;
    const color = pixelColor(pixel);

    pixelElements.push(
      <Rectangle
        key={`x:${coordinate.x} y:${coordinate.y}`}
        x={rectangleX}
        y={rectangleY}
        isOwn={pixel?.ownPixel ?? false}
        isSpawn={pixel?.type === PixelType.Spawn}
        width={mapConfig.PixelSize}
        height={mapConfig.PixelSize}
        color={color}
        onClick={() => conquer(coordinate)}
      />
    );
  }

  return (
    <>
      <Stage
        width={window.innerWidth}
        height={window.innerHeight}
        options={{ background: 0xffffff, resizeTo: window }}
      >
        <Viewport
          width={window.innerWidth}
          height={window.innerHeight}
          boundingBox={gameMapStore.pixelsBoundingBox}
        >
          <Container>{pixelElements}</Container>
        </Viewport>
      </Stage>
    </>
  );
};

export default GameMap;
