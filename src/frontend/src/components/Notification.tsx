import "./notification.css"
const Notification = ({ type, text }: { type: string; text: string }) => {
  
  if (type.length === 0) {
    type = "neutral"
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
