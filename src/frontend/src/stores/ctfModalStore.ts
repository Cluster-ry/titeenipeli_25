import { create } from "zustand";

export interface CtfModelStore {
    ctfModelOpen: boolean;

    setCtfModelOpenState: (newOpenState: boolean) => void;
}

export const useCtfStore = create<CtfModelStore>()((set) => ({
    ctfModelOpen: false,

    setCtfModelOpenState: (newOpenState: boolean) =>
        set((state: CtfModelStore) => ({
            ...state,
            ctfModelOpen: newOpenState,
        }))
}));
