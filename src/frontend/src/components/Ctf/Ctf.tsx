import { useState } from "react";
import { postCtf } from "../../api/ctf";
import { PostCtfInput } from "../../models/Post/PostCtfInput";
import "../../pages/Game/Overlay/overlay.css";
import { showErrorNotification } from "../../utils/showErrorNotification";
import { useErrorStore } from "../../stores/errorStore";
import ErrorNotification from "../ErrorNotification";

const Ctf = () => {
    const [token, setToken] = useState("");
    const CTF_DISCLAIMER = "Activate CTF";
    const { showError, updateShowError, startErrorTimer } = useErrorStore();

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
            updateShowError(true);
            startErrorTimer();
            console.log("Request unsuccessful.");
          } 
        } 
        catch (error) { 
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
            {showError && <ErrorNotification notificationText="Failed to send the CTF token."/>}
        </div>
    );
};

export default Ctf;
