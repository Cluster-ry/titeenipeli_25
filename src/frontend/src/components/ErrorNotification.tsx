import "./errorNotification.css"

const ErrorNotification = ({ notificationText }: {notificationText: string }) => {
  return (
    <div className="error_notification">
      {notificationText}
    </div>
  )
}

export default ErrorNotification;
