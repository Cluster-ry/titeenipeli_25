import { useMemo, useState } from "react";

import { Container, Stage } from "@pixi/react";
import Viewport from "./Viewport";
import Rectangle from "./Rectangle";
import { guildColor } from "./guild/Guild";
import { mapConfig } from "./MapConfig";
import { PixelMap } from "../../models/PixelMap";
import { Coordinate } from "../../models/Coordinate";
const defaultGuild = 6;   // For testing


/**
 * @component GameMap
 * 
 * Wraps up all the processes for fetching data from API for map creation.
 * Processes the data in a quadtree data structure in order to present 
 * a scalable and optimized map to guarantee a good experience for the 
 * client. 
 */
const GameMap = () => {
  
  const fovRange: number = 10;
  const playerCoordinate: Coordinate = { x: 1, y: 1 };
  const guildValues: (number | undefined)[] = [0, 1, 2, 3, 4, 5, 6, 7];  

  // A set to store existing pixels, as in a Map they need to be accessed
  // with object reference.
  const existingPixels = new Set<string>();

  /** 
   * Making sure a pixel is owned by a guild
   */  
  const checkPixelGuild = (coordinate: Coordinate, pixels: PixelMap) => {
    return guildValues.includes(pixels.get(coordinate));
  }

  // Generates an array of pixel elements
  const generatePixels = () => {
    const pixels: PixelMap = new Map();

    // Currently generated pixels with the following for-loop 
    for (let y = playerCoordinate.y - fovRange; y < playerCoordinate.y + fovRange; y++) {
      for (let x = playerCoordinate.x; x < playerCoordinate.x + fovRange; x++) {
        pixels.set({ x, y }, undefined);
        existingPixels.add(`${x},${y}`);
      }
    }
    return pixels;
  }

  const [pixelMap, setPixelMap] = useState(generatePixels());

  const pixelElements = useMemo(() => {
    const pixelElements = [];

    for (const [coordinate, guild] of pixelMap) {
      const rectangleX = coordinate.x * mapConfig.PixelSize;
      const rectangleY = coordinate.y * mapConfig.PixelSize;
      let color = guildColor(guild);

      if (!checkPixelGuild(coordinate, pixelMap)) {
        color = guildColor(undefined);
      } 
      existingPixels.add(`${coordinate.x},${coordinate.y}`)
      
      // Rendering the pixel in the form of Rectangle
      pixelElements.push(
        <Rectangle
          key={`${coordinate.x},${coordinate.y}`}
          x={rectangleX}
          y={rectangleY}
          width={mapConfig.PixelSize}
          height={mapConfig.PixelSize}
          color={color}
          onClick={() => conquer(coordinate)}
        />
      )
    }
    return pixelElements;
  }, [pixelMap])


  /**
   * Executes when the client conquers a pixel for their guild.
   * Changes the integer value representing a guild to the one
   * associated with the client's own guild.
   * 
   * @note Upon change, the map is automatically refreshed. 
   */
  function conquer(coordinate: Coordinate) {
    const newPixelMap = new Map(pixelMap);
    newPixelMap.set(coordinate, defaultGuild);
      
    // Reveal surrounding pixels within fovRange
    for (let y = coordinate.y - fovRange; y < coordinate.y + fovRange; y++) {
      for (let x = coordinate.x - fovRange; x < coordinate.x + fovRange; x++) {
        if (existingPixels.has(`${x},${y}`)) {
          continue;
        }
        newPixelMap.set({ x, y }, undefined);
      }
    }
    setPixelMap(newPixelMap);   // To be replaced by GRPC
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
          maxWidth={mapConfig.MapWidth * mapConfig.PixelSize}
          maxHeight={mapConfig.MapHeight * mapConfig.PixelSize}
        >
          <Container>{pixelElements}</Container>
        </Viewport>
      </Stage>
    </>
  );
}

export default GameMap;
