import { useEffect, useState, useCallback } from "react";
import usePixelStore from "../stores/store";
import { Pixel } from "../interfaces/Pixel";

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
  // Setting caps to manage test map boundaries
  const TEST_MAP_CAP = 3;
  const X_LOWER_CAP = 0;
  const X_UPPER_CAP = 2;
  const Y_LOWER_CAP = 0;
  const Y_UPPER_CAP = 2;

  // Local state for the elements
  const [mapElements, setMapElements] = useState<JSX.Element[][]>();

  // Global state for the data
  const { playerSpawn, pixels, setPixels, updatePlayerPosition, conquerPixel } =
    usePixelStore();

  /**
   * Filling the 2D array with default status pixels
   */
  useEffect(() => {
    const pixelArray: Pixel[][] = Array.from({ length: TEST_MAP_CAP }, () =>
      Array.from({ length: TEST_MAP_CAP }, () => ({
        type: "empty",
        owner: "",
        ownPixel: false,
      }))
    );

    setPixels(pixelArray);
  }, [setPixels]);

  /**
   * Using the previously filled 2D Pixel array to create a map
   */
  useEffect(() => {
    generateMap();
  }, [conquerPixel, pixels]);

  /**
   * A callback to handle the functionality regarding arrow key inputs
   * given by the client.
   */
  const movePlayer = useCallback(
    (direction: "up" | "down" | "left" | "right") => {
      let newX = playerSpawn.x;
      let newY = playerSpawn.y;

      switch (direction) {
        case "up":
          if (playerSpawn.y !== Y_LOWER_CAP) {
            console.log("moved up");
            newY--;
          }
          break;
        case "down":
          if (playerSpawn.y !== Y_UPPER_CAP) {
            console.log("moved down");
            newY++;
          }
          break;
        case "left":
          if (playerSpawn.x !== X_LOWER_CAP) {
            console.log("moved left");
            newX--;
          }
          break;
        case "right":
          if (playerSpawn.x !== X_UPPER_CAP) {
            console.log("moved right");
            newX++;
          }
          break;
        default:
          console.log("no movement");
      }
      updatePlayerPosition(newX, newY);
    },
    [updatePlayerPosition, playerSpawn]
  );

  /**
   * Listening to arrow key presses so that the player
   * can move around the map.
   */
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      switch (event.key) {
        case "ArrowUp":
          movePlayer("up");
          break;
        case "ArrowDown":
          movePlayer("down");
          break;
        case "ArrowLeft":
          movePlayer("left");
          break;
        case "ArrowRight":
          movePlayer("right");
          break;
        default:
          break;
      }
    };

    document.addEventListener("keydown", handleKeyDown);

    return () => {
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [movePlayer]);

  /**
   * Generating the map for the client.
   *
   * Pixel ownership illustrated by color
   * ------------------------------------
   * Red - Not Owned
   * Green - Owned
   */
  const generateMap = () => {
    const mapElems: JSX.Element[][] = pixels.map((row: Pixel[], i: number) =>
      row.map((pixel: Pixel, j: number) => (
        <div
          style={{
            border: "1px solid black",
            gridRow: i + 1,
            gridColumn: j + 1,
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
   * Conquering a pixel the user is currently on. A pixel owned by
   * the client has a green color, whereas other ones are red.
   */
  const conquer = (event) => {
    event.preventDefault();

    // For state management demonstration, an owned pixel can be disowned.
    const newOwnPixel = pixels[playerSpawn.y][playerSpawn.x].ownPixel
      ? false
      : true;
    conquerPixel("client", newOwnPixel);
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

      <div>
        <button
          id="conquer-button"
          onClick={(event) => conquer(event)}
          style={{ backgroundColor: "white", color: "black", marginTop: "1vh" }}
        >
          Conquer
        </button>
      </div>
    </div>
  );
}
