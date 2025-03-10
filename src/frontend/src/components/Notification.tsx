import "./notification.css";
import { useNotificationStore } from "../stores/notificationStore";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";

const Notification = () => {
    const { text, type, triggerNotification } = useNotificationStore();
    const [animation, setAnimation] = useState<string>("");
    const cancelRef = useRef<(() => void) | null>(null);

    const [notification, setNotification] = useState({ text: "", type: "" });

    const className = useMemo(() => {
        return `notification ${notification.type} ${animation}`;
    }, [notification.type, animation]);

    useEffect(() => {
        cancelRef.current && cancelRef.current();
        const onTimeout = (clear: boolean = false) => {
            setAnimation("");
            cancelRef.current = null;
            clear && setNotification({ text: "", type: "" });
        };
        const show = text.length > 0;
        if (show) {
            setAnimation("move-in");
            setNotification({ text, type });
        } else {
            setAnimation("move-out");
            // Timeout must match css
            const timeout = setTimeout(() => onTimeout(!show), 800);
            cancelRef.current = () => {
                setAnimation("");
                clearTimeout(timeout);
            };
        }
    }, [setAnimation, setNotification, text, type]);

    const onClick = useCallback(() => {
            triggerNotification("", "");
    }, [triggerNotification]);

    if (notification.text.length > 0) {
        return <div className={className} onClick={onClick}>{notification.text}</div>;
    }
};

export default Notification;
