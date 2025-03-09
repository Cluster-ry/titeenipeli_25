import { create } from "zustand";

export interface HelpModalStore {
    helpModalOpen: boolean;
    setHelpModalOpenState: (newOpenState: boolean) => void;
}

export const useHelpModalStore = create<HelpModalStore>()((set) => ({
    helpModalOpen: false,
    setHelpModalOpenState: (newOpenState: boolean) =>
        set((state: HelpModalStore) => ({
            ...state,
            helpModalOpen: newOpenState,
        })),
}));
