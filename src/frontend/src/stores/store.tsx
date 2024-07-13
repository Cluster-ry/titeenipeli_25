import { create } from "zustand";
import { Pixel } from "../interfaces/Pixel";
import { PlayerCoordinates } from "../interfaces/PlayerCoordinates";

interface Map {
  playerSpawn: PlayerCoordinates;
  pixels: Pixel[][];

  setPixels: (newPixels: Pixel[][]) => void;
  updatePlayerPosition: (x: number, y: number) => void;
  conquerPixel: (newOwner: string, newOwnPixel: boolean) => void;
}

const usePixelStore = create<Map>((set) => ({
  playerSpawn: { x: 0, y: 0 }, // Subject to change

  pixels: [[]],

  /**
   * Uses a Pixel[][] type array as a param to set up the map
   *
   * @param {Pixel[][]} newPixels 2D array containing the Pixel objects
   */
  setPixels: (newPixels: Pixel[][]) =>
    set((state) => ({ ...state, pixels: newPixels })),

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
