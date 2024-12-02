import { useState } from 'react';
import { postCtf } from "../api/ctf";
import { PostCtfInput } from "../models/Post/PostCtfInput";
import "../pages/Game/Overlay/overlay.css";

const Ctf = () => {
  const [disclaimer, setDisclaimer] = useState("[TEST] CTF");
  const [token, setToken] = useState("");

  const handleTokenChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setToken(event.target.value);
  };

  const handleSubmit = () => {
    const ctfToken: PostCtfInput = {
      token: token
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
        style={{pointerEvents: "all", bottom: "0"}}
      />
      <div className="ctf-icon" onClick={handleSubmit}>
        {disclaimer}
      </div>
    </div>
  );
}

export default Ctf;

