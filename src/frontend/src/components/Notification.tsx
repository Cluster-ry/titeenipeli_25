import "./notification.css"
const Notification = ({ type, text }: { type: string; text: string }) => {
  return (
    <div className={`notification ${type}`}>
      {text}
    </div>
  )
}

export default Notification;
