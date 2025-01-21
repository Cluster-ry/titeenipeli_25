import { create } from "zustand";
import { GetGameState, PixelBucket, Score, PowerUp } from "../models/Get/GetGameState";

export interface Coordinate {
    x: number;
    y: number;
}

export interface GameStateStore {
    pixelBucket: PixelBucket;
    scores: Score[];
    powerUps: PowerUp[];
    setPixelBucket: (newPixelBucket: PixelBucket) => void;
    setScores: (newScores: Score[]) => void;
    setPowerUps: (newPowerups: PowerUp[]) => void;
    setMiscGameState: (newState: GetGameState) => void;
    decreaseBucket: () => void;
    increaseBucket: () => void;
}

export const useGameStateStore = create<GameStateStore>((set) => ({
    pixelBucket: {
        amount: 0,
        maxAmount: 0,
        increasePerMinute: 0,
    },
    scores: [],
    powerUps: [],
    setPixelBucket: (newPixelBucket: PixelBucket) => {
        set({ pixelBucket: newPixelBucket });
    },
    setScores: (newScores: Score[]) => {
        set({ scores: newScores });
    },
    setPowerUps: (newPowerups: PowerUp[]) => {
        set({ powerUps: newPowerups });
    },
    setMiscGameState: (newState: GetGameState) => {
        const state = {
            pixelBucket: { ...newState.pixelBucket },
            scores: newState.scores,
            powerUps: newState.powerUps
        };
        set(state);
    },
    decreaseBucket: () => {
        set((state) => {
            const pixelBucket = {
                ...state.pixelBucket,
                amount: state.pixelBucket.amount - 1,
            };
            return { ...state, pixelBucket };
        });
    },
    increaseBucket: () => {
        set((state) => {
            const pixelBucket = {
                ...state.pixelBucket,
                amount: state.pixelBucket.amount + 1,
            };
            return { ...state, pixelBucket };
        });
    },
}));
