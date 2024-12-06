import { create } from "zustand";
import { GetGameState, PixelBucket, Score } from "../models/Get/GetGameState";

export interface Coordinate {
    x: number;
    y: number;
}

export interface GameStateStore {
    pixelBucket: PixelBucket;
    scores: Score[];
    setPixelBucket: (newPixelBucket: PixelBucket) => void;
    setScores: (newScores: Score[]) => void;
    setMiscGameState: (newState: GetGameState) => void;
    decreaseBucket: () => void;
}

export const useGameStateStore = create<GameStateStore>((set) => ({
    pixelBucket: {
        amount: 0,
        maxAmount: 0,
        increasePerMinute: 0,
    },
    scores: [],
    setPixelBucket: (newPixelBucket: PixelBucket) => {
        set({ pixelBucket: newPixelBucket });
    },
    setScores: (newScores: Score[]) => {
        set({ scores: newScores });
    },
    setMiscGameState: (newState: GetGameState) => {
        const state = {
            pixelBucket: { ...newState.pixelBucket },
            scores: newState.scores,
        };
        set(state);
    },
    decreaseBucket: () => {
        set((state) => {
            state.pixelBucket = {
                ...state.pixelBucket,
                amount: state.pixelBucket.amount - 1,
            };
            return { pixelBucket: state.pixelBucket, scores: state.scores };
        });
    },
}));
