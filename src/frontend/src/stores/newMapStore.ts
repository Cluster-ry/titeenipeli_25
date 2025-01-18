import { Pixel } from "../models/Pixel.ts";
import { create } from "zustand";

export interface Coordinate {
    x: number;
    y: number;
}

export interface NewMapStore {
    map: Map<string, Pixel | null> | null;
    setPixel: (coordinate: Coordinate, pixel: Pixel | null) => void;
    getPixel: (coordinate: Coordinate) => Pixel | undefined | null;
    setMap: (map: Map<string, Pixel> | null) => void;
    pixelsBoundingBox: { min: Coordinate; max: Coordinate };
    setPixelsBoundingBox: ({ min, max }: { min: Coordinate; max: Coordinate }) => void;
}

export const useNewMapStore = create<NewMapStore>((set) => ({
    map: null,
    setPixel: (coordinate: Coordinate, pixel: Pixel | null) => {
        set((state) => {
            // To keep updates immutable we need to do this copy operation with spread
            // If we run into performance problems on map update, this should be replaced with a mutation operation.
            // That requires careful consideration about concurrency, however.
            const oldMap = state.map;
            if (oldMap === null) {
                throw new Error("Tried to set pixel in a null map!");
            }

            if (pixel !== null) {
                return { map: new Map([...oldMap, [JSON.stringify(coordinate), pixel]]) };
            } else {
                const newMap = new Map(oldMap);
                newMap.delete(JSON.stringify(coordinate));
                return { map: newMap };
            }
        });
    },
    getPixel: (coordinate: Coordinate) => {
        let pixel: Pixel | undefined | null;
        set((state) => {
            pixel = state.map?.get(JSON.stringify(coordinate));
            return state;
        });
        return pixel;
    },
    setMap: (map: Map<string, Pixel> | null) => {
        set({ map });
    },
    pixelsBoundingBox: { min: { x: 0, y: 0 }, max: { x: 0, y: 0 } },
    setPixelsBoundingBox: ({ min, max }: { min: Coordinate; max: Coordinate }) =>
        set({ pixelsBoundingBox: { min, max } }),
}));
