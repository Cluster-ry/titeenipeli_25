import axios, { AxiosResponse, isAxiosError } from "axios";
import { PostCtfInput } from "../models/Post/PostCtfInput";
import { ClientApiError } from "../models/ClientApiError";
import { CtfOk } from "../models/CtfOk";
const CTF_URL: string = "api/v1/ctf";

export async function postCtf(postCtfInput: PostCtfInput): Promise<AxiosResponse<CtfOk> | ClientApiError> {
    try {
        console.debug(postCtfInput);
        const response = await axios.post<CtfOk>(CTF_URL, postCtfInput);
        console.debug("Success", response);
        return response as AxiosResponse<CtfOk>;
    } catch (error) {
        console.error(error);
        if (isAxiosError(error)) {
            const errorString = `${error.response?.data?.title ?? "Uh oh, I wasn't ready for that :E"}. ${error.response?.data?.description ?? "That's a weird error. Call mommy for help :("}`;
            return { msg: errorString };
        };
        return { msg: "Request unsuccessful." };
    }
}
