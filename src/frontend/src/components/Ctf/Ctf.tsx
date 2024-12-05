import { useState } from "react";
import { postCtf } from "../../api/ctf";
import { PostCtfInput } from "../../models/Post/PostCtfInput";
import "../../pages/Game/Overlay/overlay.css";
import { useErrorStore } from "../../stores/errorStore";
import ErrorNotification from "../ErrorNotification";

const Ctf = () => {
  const [notificationMessage, setNotificationMessage] = useState("");
  const [token, setToken] = useState("");
  const { showError, updateShowError, startErrorTimer } = useErrorStore();

  const CTF_DISCLAIMER: string = "Activate CTF";
  const NOTIFICATION_SUCCESS: string = "CTF activated successfully!";
  const NOTIFICATION_FAIL: string = "CTF activation failed.";

  const handleTokenChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setToken(event.target.value);
  };

  const handleSubmit = async () => {
    const ctfToken: PostCtfInput = {
      token: token,
    };
    try { 
      const result = await postCtf(ctfToken); 
      if (result.msg === "Request unsuccessful.") {
        setNotificationMessage(NOTIFICATION_FAIL);
        updateShowError(true);
        startErrorTimer();
        console.log("Request unsuccessful.");
        return;
      } else { 
        setNotificationMessage(NOTIFICATION_SUCCESS);
        updateShowError(true);
        startErrorTimer();
      }
    } 
    catch (error) {
      setNotificationMessage(NOTIFICATION_FAIL);
      updateShowError(true);
      startErrorTimer();
      console.error(error); 
    }
  };

  return (
    <div className="ctf-container">
      <input
        type="text"
        placeholder="Enter Token"
        value={token}
        onChange={handleTokenChange}
        style={{ pointerEvents: "all" }}
      />
      <button className="ctf-icon" onClick={handleSubmit}>
        {CTF_DISCLAIMER}
      </button>
      {showError && <ErrorNotification notificationText={notificationMessage}/>}
    </div>
  );
};

export default Ctf;
