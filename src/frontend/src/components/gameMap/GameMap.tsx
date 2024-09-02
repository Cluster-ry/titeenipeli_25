import { useMemo, useState } from "react";

import { Container, Stage } from "@pixi/react";
import Viewport from "./Viewport";
import Rectangle from "./Rectangle";
import { guildColor } from "./guild/Guild";
import { mapConfig } from "./MapConfig";
import { PixelMap } from "../../models/PixelMap";
import { Coordinate } from "../../models/Coordinate";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { getPixels, postPixels } from "../../api/map";
import { Pixel } from "../../models/Pixel";
const defaultGuild = 6; // For testing

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
  const generateMap = (pixelResults: Pixel[][]) => {
    const pixels: PixelMap = new Map();

    pixelResults.map((layer, y) => {
      layer.map((pixel, x) => {
        if (pixel) {
          pixels.set({ x, y }, pixel.owner);
        } else {
          pixels.set({ x, y }, undefined);
        }
      });
    });
    return pixels;
  };

  /**
   * Executes when the client conquers a pixel for their guild.
   * Changes the integer value representing a guild to the one
   * associated with the client's own guild.
   *
   * @note Upon change, the map is automatically refreshed.
   */
  async function conquer(coordinate: Coordinate) {
    let res = await postPixels(coordinate);
    console.log(res);

    /*const newPixelMap = new Map(pixelMap);
    newPixelMap.set(coordinate, defaultGuild);
    setPixelMap(newPixelMap); // To be replaced by GRPC*/
  }

  const queryClient = useQueryClient();
  const { isPending, error, data } = useQuery({
    queryKey: ["map"],
    queryFn: getPixels,
  });

  if (isPending) {
    return <span>Loading...</span>;
  }
  if (error) {
    return <span>Error: {error.message}</span>;
  }
  if ("data" in data) {
    console.log(data.data);
    let map = generateMap(data.data.pixels);
    const pixelElements = [];
    for (const [coordinate, guild] of map) {
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
            maxWidth={mapConfig.MapWidth * mapConfig.PixelSize}
            maxHeight={mapConfig.MapHeight * mapConfig.PixelSize}
          >
            <Container>{pixelElements}</Container>
          </Viewport>
        </Stage>
      </>
    );
  }
};

export default GameMap;
