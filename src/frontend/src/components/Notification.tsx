import "./notification.css";
import { useNotificationStore } from "../stores/notificationStore";

const Notification = () => {
    const { text, type } = useNotificationStore();

    if (text.length > 0) {
        return <div className={`notification ${type}`}>{text}</div>;
    }
};

export default Notification;
