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
          onClick={conquer}
        />
      )
    }
    return pixelElements;
  }, [pixelMap])


  function conquer(event: {
    x: number;
    y: number;
  }) {
    const x = event.x / 32;
    const y = event.y / 32;
  
    const newPixelMap = new Map(pixelMap);
    for (const [key] of pixelMap) {
      if (key.x === x && key.y === y) {
        console.log(pixelMap.get(key));
        newPixelMap.set(key, defaultGuild);
        console.log(newPixelMap.get(key));
        break;
      }
    }
    setPixelMap(newPixelMap);
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
