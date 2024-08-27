import { useMemo, useState } from "react";

import { Container, Stage } from "@pixi/react";
import Viewport from "./Viewport";
import Rectangle from "./Rectangle";
import { Guild, guildColor } from "./guild/Guild";
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
  // Generates an array of pixel elements
  const generatePixels = () => {
    const pixels: PixelMap = new Map();

    for (let y = 0; y < mapConfig.MapHeight; y++) {      
      for (let x = 0; x < mapConfig.MapWidth; x++) {
        const randomGuild: Guild = Math.round(Math.random() * 8);
        pixels.set({ x, y }, randomGuild);
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
      const color = guildColor(guild);
  
      pixelElements.push(
        <Rectangle 
          key={coordinate.x * 1000 + coordinate.y}
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
