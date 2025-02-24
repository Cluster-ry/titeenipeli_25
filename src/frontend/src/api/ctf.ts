import axios, { AxiosResponse } from "axios";
import { PostCtfInput } from "../models/Post/PostCtfInput";
import { ClientApiError } from "../models/ClientApiError";
import { CtfOk } from "../models/CtfOk";
const CTF_URL: string = "api/v1/ctf";

export async function postCtf(postCtfInput: PostCtfInput): Promise<AxiosResponse<CtfOk> | ClientApiError> {
    try {
        console.log(postCtfInput);
        const response = await axios.post<CtfOk>(CTF_URL, postCtfInput);
        console.log("Success", response);
        return response as AxiosResponse<CtfOk>;
    } catch (error) {
        console.error(error);
        return { msg: "Request unsuccessful." } as ClientApiError;
    }
}
