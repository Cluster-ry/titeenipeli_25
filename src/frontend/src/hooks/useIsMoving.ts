import { useRef } from "react";

export const useIsMoving = () => {
    const isMoving = useRef(false);
    const cancelRef = useRef<(() => void) | null>(null);
    
    const toggle = () => {
        isMoving.current = false;
        cancelRef.current = null;
    };
    const cancel = (timeoutId: NodeJS.Timeout) => {
        clearTimeout(timeoutId);
    }

    const startMoving = () => {
        isMoving.current = true;
        cancelRef.current && cancelRef.current();
        const timeout = setTimeout(toggle, 300);
        cancelRef.current = (() => cancel(timeout));
    };
  
    return { isMoving, startMoving };
};
