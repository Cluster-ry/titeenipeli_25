import { Pixel } from "../models/Pixel.ts";
import { create } from "zustand";

export interface Coordinate {
    x: number;
    y: number;
}

export interface NewMapStore {
    map: Map<Coordinate, Pixel | null> | null;
    setPixel: (coordinate: Coordinate, pixel: Pixel | null) => void;
    setMap: (map: Map<Coordinate, Pixel> | null) => void;
    pixelsBoundingBox: { min: Coordinate; max: Coordinate };
    setPixelsBoundingBox: ({ min, max }: { min: Coordinate; max: Coordinate }) => void;
}

export const useNewMapStore = create<NewMapStore>((set, get) => ({
    map: null,
    setPixel: (coordinate: Coordinate, pixel: Pixel | null) => {
        const oldMap = get().map;
        if (oldMap === null) {
            throw new Error("Tried to set pixel in a null map!");
        }
        // To keep updates immutable we need to do this copy operation with spread
        // If we run into performance problems on map update, this should be replaced with a mutation operation.
        // That requires careful consideration about concurrency, however.
        set({ map: new Map([...oldMap, [coordinate, pixel]]) });
    },
    setMap: (map: Map<Coordinate, Pixel> | null) => {
        set({ map });
    },
    pixelsBoundingBox: { min: { x: 0, y: 0 }, max: { x: 0, y: 0 } },
    setPixelsBoundingBox: ({ min, max }: { min: Coordinate; max: Coordinate }) =>
        set({ pixelsBoundingBox: { min, max } }),
}));
