import { useState } from "react";
import { postCtf } from "../../api/ctf";
import { PostCtfInput } from "../../models/Post/PostCtfInput";
import "../../pages/Game/Overlay/overlay.css";
import { showErrorNotification } from "../../utils/showErrorNotification";

const Ctf = () => {
    const [token, setToken] = useState("");
    const CTF_DISCLAIMER = "Activate CTF";

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
            console.log("Request unsuccessful."); 
            showErrorNotification(); 
          } 
        } 
        catch (error) { 
          console.error("Error occurred:", error); 
          showErrorNotification();
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
