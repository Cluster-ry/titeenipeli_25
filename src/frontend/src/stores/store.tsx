import { create } from "zustand";
import { Pixel } from "../models/Pixel";
import { PlayerCoordinates } from "../models/PlayerCoordinates";
import { Guild } from "../components/gameMap/guild/Guild";
import { mapConfig } from "../components/gameMap/MapConfig";


// The amount of rows and columns in the map. These can be
// changed to alter the map size.

interface Map {
  playerSpawn: PlayerCoordinates;
  playerGuild: Guild;
  pixels: Pixel[][];
  mapSet: boolean;

  setPixels: () => void;
  setPlayerGuild: (guild: number) => void; 
  updatePlayerPosition: (x: number, y: number) => void;
  conquerPixel: (newOwner: number, newOwnPixel: boolean) => void;
}

const usePixelStore = create<Map>((set) => ({
  playerSpawn: { x: 0, y: 0 }, // Subject to change
  playerGuild: 2,   // Cluster as default for TESTING
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
      for (let y = 0; y < mapConfig.MapHeight - 1; y++) {
        pixels.push([]);
      }

      // Creating a the specified amount of pixels for each row, with each
      // containing the default values.
      pixels.forEach((pixelRow) => {
        for (let x = 0; x < mapConfig.MapWidth; x++) {
          pixelRow.push({
            type: "empty",
            owner: Math.round(Math.random() * 8) as Guild,
            ownPixel: false,
          });
        }
      });

      return { ...state, pixels: pixels, mapSet: true };
    }),

    setPlayerGuild: (guild: number) =>
      set((state) => {

        return {...state, playerGuild: guild}
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
  conquerPixel: (newOwner: number, newOwnPixel: boolean) =>
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
