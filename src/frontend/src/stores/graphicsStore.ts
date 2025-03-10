/**
 * A store that has the graphics visibility state stored in local storage.
 * The intention is to allow the client to either hide the background
 * graphics or make them visible.
 *
 * - Visible by default
 */

import { create } from "zustand";
import { persist } from "zustand/middleware";

interface GraphicsStore {
    graphicsEnabled: boolean;
    setGraphicsEnabled: (newGraphicsEnabled: boolean) => void;
}

export const useGraphicsStore = create<GraphicsStore>()(
    persist(
        (set) => ({
            graphicsEnabled: true,

            setGraphicsEnabled: (newGraphicsEnabled: boolean) =>
                set((state: GraphicsStore) => ({
                    ...state,
                    graphicsEnabled: newGraphicsEnabled,
                })),
        }),
        {
            name: "graphics-storage",
        },
    ),
);
