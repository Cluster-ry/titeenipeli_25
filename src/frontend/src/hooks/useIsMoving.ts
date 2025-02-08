import { useRef } from "react";

export const useIsMoving = () => {
    const isMoving = useRef(false);
    const delay = useRef<Promise<void> | null>(null);
    
    const startMoving = () => {
        delay.current = null;
        isMoving.current = true;
    };

    const stopMoving = async () => {
        if (delay.current) return;
        delay.current = new Promise((resolve) => {
            const toggle = () => {
                isMoving.current = false;
                resolve();
            };
            setTimeout(toggle, 300);
        });
        await delay.current;
        delay.current = null;
    };

    return { isMoving, startMoving, stopMoving };
};