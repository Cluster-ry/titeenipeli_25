import { useEffect, useMemo, useState } from "react";

import { Container, Stage } from "@pixi/react";
import Viewport from "./Viewport";
import Rectangle from "./Rectangle";
import usePixelStore from "../../stores/store";
import { Pixel } from "../../models/Pixel";
import { Guild, guildColor } from "./guild/Guild";

const MapWidth = 256;
const MapHeight = 256;

const PixelSize = 32;

const MaxWidth = MapWidth * PixelSize;
const MaxHeight = MapHeight * PixelSize;

export default function GameMap() {
  // Local state for the elements
  const [mapElements, setMapElements] = useState<JSX.Element[][]>();

  // Global state for the data
  const { playerSpawn, pixels, setPixels, updatePlayerPosition, conquerPixel } =
    usePixelStore();

  /**
   * Filling the 2D array with default status pixels
   */
  useEffect(() => {
    setPixels();
  }, [setPixels]);

  /**
   * Using the previously filled 2D Pixel array to create a map
   */
  useEffect(() => {
    generateMap();
  }, [pixels, playerSpawn]);

  const generateMap = () => {
    const mapElems: JSX.Element[][] = pixels.map((row: Pixel[], y: number) =>
      row.map((pixel: Pixel, x: number) => (
        <Rectangle
          key={x * 1000 + y}
          x={x * PixelSize}
          y={y * PixelSize}
          width={PixelSize}
          height={PixelSize}
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
    updatePlayerPosition(event.x / PixelSize, event.y / PixelSize);
    if (!pixels[event.x / PixelSize][event.y / PixelSize].ownPixel) {
      conquerPixel(
        0,
        !pixels[event.x / PixelSize][event.y / PixelSize].ownPixel
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
