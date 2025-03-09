import { create } from "zustand";

export interface NotificationStore {
    text: string;
    type: string;
    timeoutId: NodeJS.Timeout | null;

    updateText: (newText: string) => void;
    updateType: (newType: string) => void;
    triggerNotification: (newText: string, newType: string) => void;
}

export const useNotificationStore = create<NotificationStore>()((set) => ({
    text: "",
    type: "",
    timeoutId: null,

    updateText: (newText: string) =>
        set((state: NotificationStore) => ({
            ...state,
            text: newText,
        })),

    updateType: (newType: string) =>
        set((state: NotificationStore) => ({
            ...state,
            type: newType,
        })),

    triggerNotification: (newText: string, newType: string) => {
        set((state: NotificationStore) => {
            if (state.timeoutId !== null) {
                clearTimeout(state.timeoutId);
            };
            const timeoutId = setTimeout(() => {
                set((state: NotificationStore) => ({
                    ...state,
                    type: "",
                    text: "",
                    timeoutId: null
                }));
            }, 10000);
            return {
                ...state,
                text: newText,
                type: newType,
                timeoutId,
            }
        });
    },
}));
