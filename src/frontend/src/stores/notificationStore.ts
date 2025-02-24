import { create } from "zustand";

export interface NotificationStore {
    text: string;
    type: string;

    updateText: (newText: string) => void;
    updateType: (newType: string) => void;
    triggerNotification: (newText: string, newType: string) => void;
}

export const useNotificationStore = create<NotificationStore>()((set) => ({
    text: "",
    type: "",

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
        set((state: NotificationStore) => ({
            ...state,
            text: newText,
            type: newType,
        })),
            setTimeout(() => {
                set((state: NotificationStore) => ({
                    ...state,
                    type: "",
                    text: "",
                }));
            }, 10000);
    },
}));
