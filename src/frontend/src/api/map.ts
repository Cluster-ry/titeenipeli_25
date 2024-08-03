import axios, { AxiosResponse } from "axios";
import { PostPixelsInput } from "../models/PostPixelsInput";
import { ClientApiError } from "../models/ClientApiError";

const PIXELS_URL = "http://localhost:8080/map/pixels";

export async function getPixels(): Promise<AxiosResponse<PostPixelsInput, any> | ClientApiError> {
    try {
        const response = await axios.get<PostPixelsInput>(
            PIXELS_URL,
        );

        console.log("Success.");
        return response;

    } catch(error) {
        console.error(error);
        return { msg: "Request unsuccessful." };
    }
}

export async function postPixels(postPixelsInput: PostPixelsInput): Promise<AxiosResponse<PostPixelsInput, any> | ClientApiError> {
    try {
        const response = axios.post<PostPixelsInput>(
            PIXELS_URL,
            postPixelsInput,
        )

        console.log("Success.");
        return response;
    } catch(error) {
        console.error(error);
        return { msg: "Request unsuccessful." };
    }
}