import { create } from "zustand";
import { Coordinate } from "../models/Coordinate";
import { activatePowerUp } from "../api/powerup";

export interface PowerUpStore {
    powerUp: number | null;
    location: Coordinate | null;
    setPowerUp: (id: number | null) => void;
    usePowerUp: (coordinate: Coordinate) => boolean;
}

enum Direction {
    North = 1,
    West = 2,
    South = 3,
    East = 4,
    None = 0,
}

const getDirection = (center: Coordinate, target: Coordinate) => {
    const directionX = center.x - target.x;
    const directionY = center.y - target.y;
    if (directionX === 0 && directionY === 0) return Direction.None;
    const direction = Math.abs(directionX) > Math.abs(directionY)
    ? (directionX > 0 ? Direction.West : Direction.East)
    : (directionY > 0 ? Direction.North : Direction.South);
    return direction;
};

export const usePowerUpStore = create<PowerUpStore>((set, get) => ({
    powerUp: null,
    location: null,
    setPowerUp: (value: number | null) => {
        set(state => {
            return { ...state, powerUp: value === state.powerUp ? null : value };
        })
    },
    usePowerUp: (coordinate: Coordinate, ) => {
        const state = get();
        console.log(state);
        if (state.powerUp === null) return false;
        if (state.location === null) {
            set(prev => ({ ...prev, location: coordinate }));
            return true;
        }
        const direction = getDirection(state.location, coordinate);
        if (direction === 0) return false;
        activatePowerUp({ id: state.powerUp, location: state.location, direction });
        set(prev => ({ ...prev, powerUp: null, location: null }))
        return true;
    }
}));
