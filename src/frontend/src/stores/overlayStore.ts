import { create } from "zustand";
import { persist } from "zustand/middleware";

export interface OverlayStore {
  showHelp: boolean;
  updateShowHelp: (showHelp: boolean) => void;
}

export const useOverlayStore = create<OverlayStore>()(
  persist(
    (set) => ({
      showHelp: false,

      updateShowHelp: (newShowHelp: boolean) => 
        set((state: OverlayStore) => ({
          ...state,
          showHelp: newShowHelp,
        })),
    }),
    {
      name: "overlay-store"
    }
  )
);

