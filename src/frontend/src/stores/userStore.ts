import { User } from "../models/User.ts";
import { create } from "zustand";

export interface UserStore {
    user: User | null;
    setUser: (newUser: User) => void;
}

export const useUserStore = create<UserStore>((set) => ({
    user: null,
    setUser: (newUser: User) => set({ user: newUser }),
}));
