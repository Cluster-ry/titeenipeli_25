import { postCtf } from "../api/ctf";
import { PostCtfInput } from "../models/Post/PostCtfInput";

const Ctf = () => {
  const TEST_DISCLAIMER = "[TEST] CTF";
 
  // NOTE! IMPLEMENTATION COULD BE WORTH CHANGING IN THE FUTURE!
  const ctfToken: PostCtfInput = {
    token: "FGSTLBGXM3YB7USWS28KE2JV9Z267L48"
  }
  
  return (
    <div onClick={() => postCtf(ctfToken)} style={{width: "200px", height: "100px"}}>
      {TEST_DISCLAIMER}
    </div>
  );
}

export default Ctf;
