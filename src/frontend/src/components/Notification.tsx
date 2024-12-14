import "./notification.css"
import { useNotificationStore } from "../stores/notificationStore";

const NEUTRAL_TYPE = "neutral";

const Notification = () => {
  const {text, type, updateType} = useNotificationStore(); 

  // For edge cases where the type has not been defined
  if (type.length === 0) {
    updateType(NEUTRAL_TYPE);
  } 

  if (text.length > 0) {
    return (
      <div className={`notification ${type}`}>
        {text}
      </div>
    )
  } 
}

export default Notification;
