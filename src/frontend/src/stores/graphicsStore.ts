import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface GraphicsStore {
  graphicsEnabled: boolean;
  setGraphicsEnabled: (newGraphicsEnabled: boolean) => void;
}

export const graphicsStore = create<GraphicsStore>()(
  persist(
    (set) => ({
      graphicsEnabled: false,

      setGraphicsEnabled: (newGraphicsEnabled: boolean) =>
        set((state: GraphicsStore) => ({
          ...state,
          graphicsEnabled: newGraphicsEnabled,
        })),
    }),
    {
      name: "graphics-storage",
    }
  )
);

