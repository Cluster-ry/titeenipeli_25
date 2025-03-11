import { create } from "zustand";
import { Coordinate } from "../models/Coordinate";

export interface PowerUpStore {
    powerUp: number | null;
    target: Coordinate | null;
    setPowerUp: (id: number | null, keyAsId: string | null) => void;
    setTarget: (target: Coordinate) => void;
    resetPowerUp: () => void;
    uiPowerUpId: string | null;
}

export const usePowerUpStore = create<PowerUpStore>((set) => ({
    powerUp: null,
    target: null,
    uiPowerUpId: null,
    setPowerUp: (value: number | null, keyAsId: string | null) => {
        set((state) => {
            const cancel = value === state.powerUp;
            return { ...state, powerUp: cancel ? null : value, target: cancel ? null : state.target, uiPowerUpId: cancel ? null : keyAsId };
        })
    },
    setTarget: (value: Coordinate) => {
        set((state) => {
            return { ...state, target: value };
        });
    },
    resetPowerUp: () => {
        set((state) => {
            return { ...state, powerUp: null, target: null, uiPowerUpId: null };
        });
    },
}));
