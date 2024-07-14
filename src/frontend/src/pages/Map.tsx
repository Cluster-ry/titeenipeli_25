import { useEffect, useState } from "react";
import usePixelStore from "../stores/store";
import { Pixel } from "../models/Pixel";

/**
 * Map component
 *
 * Displaying the game map view
 *
 * @CurrentStatus
 *
 * The program is currently designed in a way that demonstrates
 * the functionality of the state management with Zustand.
 */
export default function Map() {
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

  /**
   * Generating the map for the client.
   *
   * Pixel ownership illustrated by color
   * ------------------------------------
   * Red - Not Owned
   * Green - Owned
   */
  const generateMap = () => {
    const mapElems: JSX.Element[][] = pixels.map((row: Pixel[], y: number) =>
      row.map((pixel: Pixel, x: number) => (
        <div
          onClick={() => movePlayerAndConquer(x, y)}
          style={{
            border: "1px solid black",
            gridRow: y + 1,
            gridColumn: x + 1,
            backgroundColor: pixel.ownPixel ? "green" : "red",
            width: "40px",
            height: "40px",
          }}
        ></div>
      ))
    );

    setMapElements(mapElems);
  };

  /**
   * Moving the user to the clicked pixel and conquering it.
   *
   * @NOTE Currently also allows disowning a pixel that is
   * clicked in order to demonstrate the state management
   * functionality. If this feature is deemed undesirable,
   * it can be removed.
   *
   * @param {number} x The x-axis coordinate
   * @param {number} y The y-axis coordinate
   */
  const movePlayerAndConquer = (x: number, y: number) => {
    updatePlayerPosition(x, y);
    conquerPixel("client", !pixels[y][x].ownPixel);
  };

  return (
    <div id="map">
      <div>
        Coordinates
        <p>
          x: {playerSpawn.x} y: {playerSpawn.y}
        </p>
      </div>
      <div style={{ display: "grid" }}>{mapElements}</div>
    </div>
  );
}
