import "./notification.css";
import { useNotificationStore } from "../stores/notificationStore";
import { useCallback, useEffect, useMemo, useState } from "react";

enum Animation {
    MoveIn = "move-in",
    MoveOut = "move-out",
    None = ""
}

const Notification = () => {
    const triggerNotification = useNotificationStore(state => state.triggerNotification);
    const text = useNotificationStore(state => state.text);
    const type = useNotificationStore(state => state.type);
    const [animation, setAnimation] = useState<Animation>(Animation.None);
    const [notification, setNotification] = useState({ text: "", type: "" });

    const className = useMemo(() => {
        return `notification ${notification.type} ${animation}`;
    }, [notification.type, animation]);

    const onAnimationEnd = useCallback(() => {
        setAnimation(prev => {
            if (prev === Animation.MoveOut) setNotification({ text: "", type: "" });
            return Animation.None;
        });
    }, [setAnimation, setNotification]);

    useEffect(() => {
        const show = text.length > 0;
        if (show) {
            setAnimation(Animation.MoveIn);
            setNotification({ text, type });
        } else {
            setAnimation(Animation.MoveOut);
        }
    }, [setAnimation, setNotification, text, type]);

    const onClick = useCallback(() => {
            triggerNotification("", "");
    }, [triggerNotification]);

    if (notification.text.length > 0) {
        return <div className={className} onAnimationEnd={onAnimationEnd} onClick={onClick}>{notification.text}</div>;
    }
};

export default Notification;
