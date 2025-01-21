import { create } from "zustand";
import { Coordinate } from "../models/Coordinate";

export interface PowerUpStore {
    powerUp: number | null;
    target: Coordinate | null;
    setPowerUp: (id: number | null) => void;
    setTarget: (target: Coordinate) => void;
    resetPowerUp: () => void;
}

export const usePowerUpStore = create<PowerUpStore>((set) => ({
    powerUp: null,
    target: null,
    setPowerUp: (value: number | null) => {
        console.log("Tere");
        set(state => {
            const cancel = value === state.powerUp;
            console.log(value);
            return { ...state, powerUp: cancel ? null : value, target: cancel ? null : state.target  };
        })
    },
    setTarget: (value: Coordinate) => {
        console.log("Turus");
        set(state => {
            return { ...state, target: value  };
        })
    },
    resetPowerUp: () => {
        console.log("Moro");
        set(state => {
            return { ...state, powerUp: null, target: null  };
        })
    },
}));
