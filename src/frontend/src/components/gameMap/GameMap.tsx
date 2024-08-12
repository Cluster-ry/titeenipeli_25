import { useMemo } from "react";

import { Container, Stage } from "@pixi/react";
import Viewport from "./Viewport";
import Rectangle from "./Rectangle";

const MapWidth = 256;
const MapHeight = 256;

const PixelSize = 32;

const MaxWidth = MapWidth * PixelSize
const MaxHeight = MapHeight * PixelSize

enum Guild {
  Tietokilta = 0,
  Algo = 1,
  Cluster = 2,
  OulunTietoteekkarit = 3,
  TietoTeekkarikilta = 4,
  Digit = 5,
  Datateknologerna = 6,
  Sosa = 7,
}

interface Coordinate {
  x: number;
  y: number;
}

type PixelMap = Map<Coordinate, Guild | undefined>;

export default function GameMap() {
  const pixels = useMemo(generatePixels, []);

  function generatePixels() {
    const pixels: PixelMap = new Map();
    for (let y = 0; y < MapHeight; y++) {
      for (let x = 0; x < MapWidth; x++) {
        const randomGuild = Math.round(Math.random() * 8) as Guild;
        pixels.set({ x, y }, randomGuild);
      }
    }
    return pixels;
  }

  const pixelElements = [];
  for (const [coordinate, guild] of pixels) {
    const rectangleX = coordinate.x * PixelSize;
    const rectangleY = coordinate.y * PixelSize;
    const color = guildColor(guild)

    pixelElements.push(
      <Rectangle
        key={coordinate.x * 1000 + coordinate.y}
        x={rectangleX}
        y={rectangleY}
        width={PixelSize}
        height={PixelSize}
        color={color}
      />
    );
  }

  function guildColor(guild: Guild | undefined): number {
    switch (guild) {
      case Guild.Tietokilta:
        return 0xd50000;
      case Guild.Algo:
        return 0xC51162;
      case Guild.Cluster:
        return 0xAA00FF;
      case Guild.OulunTietoteekkarit:
        return 0x6200EA;
      case Guild.TietoTeekkarikilta:
        return 0x304FFE;
      case Guild.Digit:
        return 0x2962FF;
      case Guild.Datateknologerna:
        return 0x0091EA;
      case Guild.Sosa:
        return 0x00B8D4;
      default:
        return 0x000000;
    }
  }

  return (
    <Stage
      width={window.innerWidth}
      height={window.innerHeight}
      options={{ background: 0xffffff, resizeTo: window }}
    >
      <Viewport width={window.innerWidth} height={window.innerHeight} maxWidth={MaxWidth} maxHeight={MaxHeight}>
        <Container>
          {pixelElements}
        </Container>
      </Viewport>
    </Stage>
  );
}
