import { create } from "zustand";

export interface OverlayStore {
  showHelp: boolean;
  updateShowHelp: (showHelp: boolean) => void;
}

export const useOverlayStore = create<OverlayStore>()(
  (set) => ({
    showHelp: false,

    updateShowHelp: (newShowHelp: boolean) => 
      set((state: OverlayStore) => ({
        ...state,
        showHelp: newShowHelp,
      })),
  }),
);

