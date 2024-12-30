import axios, { AxiosResponse } from "axios";
import { PostCtfInput } from "../models/Post/PostCtfInput";
import { ClientApiError } from "../models/ClientApiError";
import { ClientApiOk } from "../models/ClientApiOk";
const CTF_URL: string = "api/v1/ctf";

export async function postCtf(postCtfInput: PostCtfInput): Promise<AxiosResponse<ClientApiOk> | ClientApiError> {
    try {
        console.log(postCtfInput);
        const response = await axios.post<ClientApiOk>(CTF_URL, postCtfInput);
        console.log("Success");
        return response as AxiosResponse<ClientApiOk>;
    } catch (error) {
        console.error(error);
        return { msg: "Request unsuccessful." } as ClientApiError;
    }
}
