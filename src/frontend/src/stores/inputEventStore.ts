import { create } from "zustand";

export interface InputEventStore {
    moving: boolean;
    moveEnded: Date;
    setMoving: (newPinchingState: boolean) => void;
}

export const useInputEventStore = create<InputEventStore>((set) => ({
    moving: true,
    moveEnded: new Date(0),
    setMoving: (newMovingState: boolean) => {
        if (newMovingState) {
            set({ moving: newMovingState });
        } else {
            set({ moving: newMovingState, moveEnded: new Date() });
        }
    },
}));
