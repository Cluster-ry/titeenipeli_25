import axios, { AxiosResponse } from "axios";
import { PostCtfInput } from "../models/Post/PostCtfInput";
import { ClientApiError } from "../models/ClientApiError";

const CTF_URL = "http://localhost:8080/ctf";

export async function postCtf(postCtfInput: PostCtfInput): Promise<AxiosResponse<PostCtfInput> | ClientApiError> {
    try {
        const response = await axios.post<PostCtfInput>(
            CTF_URL,
            postCtfInput,
        );

        console.log("Success");
        return response;
        
    } catch(error) {
        console.error(error);
        return { msg: "Request unsuccessful." };
    }
}