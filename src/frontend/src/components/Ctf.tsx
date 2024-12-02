import { postCtf } from "../api/ctf";
import { PostCtfInput } from "../models/Post/PostCtfInput";
import "../pages/Game/Overlay/overlay.css"; 

const Ctf = () => {
  const TEST_DISCLAIMER = "[TEST] CTF";
 
  // NOTE! IMPLEMENTATION COULD BE WORTH CHANGING IN THE FUTURE!
  const ctfToken: PostCtfInput = {
    token: "#TEST_FLAG"
  } 
  
  return (
    <div className="ctf-icon" onClick={() => postCtf(ctfToken)}>
      {TEST_DISCLAIMER}
    </div>
  );
}

export default Ctf;
