import { useState } from "react";
import { postCtf } from "../api/ctf";
import { PostCtfInput } from "../models/Post/PostCtfInput";
import "../pages/Game/Overlay/overlay.css";

const Ctf = () => {
    const [token, setToken] = useState("");

    const CTF_DISCLAIMER = "[TEST] CTF";

    const handleTokenChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setToken(event.target.value);
    };

    const handleSubmit = () => {
        const ctfToken: PostCtfInput = {
            token: token,
        };
        postCtf(ctfToken);
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
            <div className="ctf-icon" onClick={handleSubmit}>
                {CTF_DISCLAIMER}
            </div>
        </div>
    );
};

export default Ctf;
