import { useState } from "react";
import { postCtf } from "../../api/ctf";
import { PostCtfInput } from "../../models/Post/PostCtfInput";
import "../../pages/Game/Overlay/overlay.css";
import { useNotificationStore } from "../../stores/notificationStore";

const Ctf = () => {
  const [token, setToken] = useState("");
  const { triggerNotification } = useNotificationStore();

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
        triggerNotification(NOTIFICATION_FAIL, "error");
        console.log("Request unsuccessful.");
        return;
      } else { 
        triggerNotification(NOTIFICATION_SUCCESS, "success");
      }
    } 
    catch (error) {
      triggerNotification(NOTIFICATION_FAIL, "error");
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
    </div>
  );
};

export default Ctf;
