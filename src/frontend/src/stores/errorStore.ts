import { create } from "zustand";

export interface ErrorStore {
  showError: boolean;
  updateShowError: (showError: boolean) => void;
  startErrorTimer: () => void;
}

export const useErrorStore = create<ErrorStore>()((set) => ({
  showError: false,

  updateShowError: (newShowError: boolean) =>
    set((state: ErrorStore) => ({
      ...state,
      showError: newShowError,
    })),
  startErrorTimer: () => { 
    setTimeout(() => { 
      set((state: ErrorStore) => ({ 
        ...state, 
        showError: false, 
      })); 
    }, 5000); 
  },
}));
