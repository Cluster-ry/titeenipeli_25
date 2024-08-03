import axios from "axios";
import { PostCtfInput } from "../models/PostCtfInput";

const CTF_URL = "http://localhost:8080/ctf";

export async function postCtf(postCtfInput: PostCtfInput) {
    try {
        return await axios.post<PostCtfInput>(
            CTF_URL,
            postCtfInput,
        )
        .then( response => {
            console.log(response)       // REMOVE THIS
            if (response.status === 200) {
                console.log("Success.");
                return;
            }
            console.log("Failure.");
        });
    } catch(error) {
        console.error(error);
    }
}