import axios, { AxiosHeaders } from "axios";
import { PostCtfInput } from "../models/PostCtfInput";

const CTF_URL = "/ctf";

export async function postCtf(postCtfInput: PostCtfInput) {
    try {
        const headers = new AxiosHeaders();
        return await axios.post<PostCtfInput>(
            CTF_URL,
            postCtfInput,
            {
                headers
            }
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