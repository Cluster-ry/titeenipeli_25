import { useEffect, useState } from "react";

import { Container, Stage } from "@pixi/react";
import Viewport from "./Viewport";
import Rectangle from "./Rectangle";
import usePixelStore from "../../stores/store";
import { Pixel } from "../../models/Pixel";
import { Guild, guildColor } from "./guild/Guild";

import { mapConfig } from "./MapConfig";

const MaxWidth = mapConfig.MapWidth * mapConfig.PixelSize;
const MaxHeight = mapConfig.MapHeight * mapConfig.PixelSize;

const defaultGuild = 0;   // For testing

export default function GameMap() {
  // Local state for the elements
  const [mapElements, setMapElements] = useState<JSX.Element[][]>();

  // Global state for the data
  const { playerSpawn, pixels, playerGuild, setPixels, setPlayerGuild, updatePlayerPosition, conquerPixel } =
    usePixelStore();

  /**
   * Filling the 2D array with default status pixels
   */
  useEffect(() => {
    setPixels();
  }, [setPixels]);

  /**
   * Using the previously filled 2D Pixel array to create a map
   * 
   * @note Currently sets the player's guild as well.
   */
  useEffect(() => {
    generateMap();
    setPlayerGuild(defaultGuild);
  }, [pixels, playerSpawn]);

  const generateMap = () => {
    const mapElems: JSX.Element[][] = pixels.map((row: Pixel[], y: number) =>
      row.map((pixel: Pixel, x: number) => (
        <Rectangle
          key={x * 1000 + y}
          x={x * mapConfig.PixelSize}
          y={y * mapConfig.PixelSize}
          width={mapConfig.PixelSize}
          height={mapConfig.PixelSize}
          color={guildColor(pixel.owner as Guild)}
          onClick={movePlayerAndConquer}
        />
      ))
    );

    setMapElements(mapElems);
  };

  function movePlayerAndConquer(event: {
    x: number;
    y: number;
    color: number;
  }) {
    updatePlayerPosition(event.x / mapConfig.PixelSize, event.y / mapConfig.PixelSize);
    if (!pixels[event.x / mapConfig.PixelSize][event.y / mapConfig.PixelSize].ownPixel) {
      conquerPixel(
        playerGuild,
        !pixels[event.x / mapConfig.PixelSize][event.y / mapConfig.PixelSize].ownPixel
      );
    }
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
          maxWidth={MaxWidth}
          maxHeight={MaxHeight}
        >
          <Container>{mapElements}</Container>
        </Viewport>
      </Stage>
    </>
  );
}
