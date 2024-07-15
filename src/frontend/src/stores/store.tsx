import { create } from "zustand";
import { Pixel } from "../models/Pixel";
import { PlayerCoordinates } from "../models/PlayerCoordinates";

// The amount of rows and columns in the map. These can be
// changed to alter the map size.
const ROW_COUNT = 3;
const COLUMN_COUNT = 3;

interface Map {
  playerSpawn: PlayerCoordinates;
  pixels: Pixel[][];
  mapSet: boolean;

  setPixels: () => void;
  updatePlayerPosition: (x: number, y: number) => void;
  conquerPixel: (newOwner: string, newOwnPixel: boolean) => void;
}

const usePixelStore = create<Map>((set) => ({
  playerSpawn: { x: 0, y: 0 }, // Subject to change
  pixels: [[]],
  mapSet: false,

  /**
   * Sets up the map
   *
   * Sets up the pixel array with the JSON objects that represent
   * the data each pixel contains. Then the mapSet boolean status
   * is changed to prevent the creation of duplicate maps.
   */
  setPixels: () =>
    set((state) => {
      // Checking for previous setups to prevent duplicate map creation.
      if (state.mapSet) {
        return { ...state };
      }

      const pixels = [...state.pixels];

      // Creating the rows for the 2D array
      // -1 is in the middle due to the 2D array being initialized with 1 row.
      for (let y = 0; y < ROW_COUNT - 1; y++) {
        pixels.push([]);
      }

      // Creating a the specified amount of pixels for each row, with each
      // containing the default values.
      pixels.forEach((pixelRow) => {
        for (let x = 0; x < COLUMN_COUNT; x++) {
          pixelRow.push({
            type: "empty",
            owner: "",
            ownPixel: false,
          });
        }
      });

      return { ...state, pixels: pixels, mapSet: true };
    }),

  /**
   * Updating the coordinates of the client/player
   *
   * @param {number} newX New x-axis coordinate
   * @param {number} newY New y-axis coordinate
   */
  updatePlayerPosition: (newX: number, newY: number) =>
    set((state) => ({
      ...state,
      playerSpawn: { x: newX, y: newY },
    })),

  /**
   * Changing the state of a pixel that is conquered by the client
   *
   * @param {number} index The way to identify the pixel that
   * is conquered. [THIS IS A PLACEHOLDER SOLUTION]
   *
   * @param {boolean} newOwnPixel The new ownership status
   */
  conquerPixel: (newOwner: string, newOwnPixel: boolean) =>
    set((state) => {
      const pixels = [...state.pixels];
      const { x, y } = state.playerSpawn;
      pixels[y][x] = {
        ...pixels[y][x],
        owner: newOwner,
        ownPixel: newOwnPixel,
      };
      return { ...state, pixels: pixels };
    }),
}));

export default usePixelStore;
