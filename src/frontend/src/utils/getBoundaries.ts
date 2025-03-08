import { Coordinate } from "../models/Coordinate";

export const getBoundaries = (
    coordinates: Coordinate[],
    startingValues = {
        minX: Number.MAX_SAFE_INTEGER,
        minY: Number.MAX_SAFE_INTEGER,
        maxX: Number.MIN_SAFE_INTEGER,
        maxY: Number.MIN_SAFE_INTEGER
    }) => {
    let minX = startingValues.minX;
    let minY = startingValues.minY;
    let maxX = startingValues.maxX;
    let maxY = startingValues.maxY;
    for (const { x, y } of coordinates) {
        if (x < minX) (minX = x);
        if (y < minY) (minY = y);
        if (x > maxX) (maxX = x);
        if (y > maxY) (maxY = y);
    }
    return {
        min: {
            x: minX,
            y: minY,
        },
        max: {
            x: maxX,
            y: maxY,
        },
    };
};