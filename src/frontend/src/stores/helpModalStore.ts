import { create } from "zustand";

export interface HelpModalStore {
    helpModalOpen: boolean;
    setHelpModalOpenState: (newOpenState: boolean) => void;
}

export const helpModalStore = create<HelpModalStore>()((set) => ({
    helpModalOpen: false,
    setHelpModalOpenState: (newOpenState: boolean) =>
        set((state: HelpModalStore) => ({
            ...state,
            helpModalOpen: newOpenState,
        })),
}));
