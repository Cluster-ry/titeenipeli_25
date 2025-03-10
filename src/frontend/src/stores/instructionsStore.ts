import { create } from "zustand";

export interface InstrustionsStore {
    instructionsOn: boolean;
    setInstructionsOn: (newVisible: boolean) => void;
}

export const useInstructionsStore = create<InstrustionsStore>()((set) => ({
    instructionsOn: false,
    setInstructionsOn: (newInstructionsOn: boolean) =>
        set((state: InstrustionsStore) => ({
            ...state,
            instructionsOn: newInstructionsOn,
        })),
}));
