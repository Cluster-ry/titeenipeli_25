import axios, { AxiosResponse } from "axios";
import { PostPixelsInput } from "../models/Post/PostPixelsInput";
import { GetPixelsResult } from "../models/Get/GetPixelsResult";
import { ClientApiError } from "../models/ClientApiError";
import { ClientApiOk } from "../models/ClientApiOk";

const PIXELS_URL = "api/v1/state/map/pixels";

export async function getPixels() {
    return await axios.get<GetPixelsResult>(PIXELS_URL);
}

export async function postPixels(postPixelsInput: PostPixelsInput): Promise<boolean> {
    try {
        await axios.post<ClientApiOk>(PIXELS_URL, postPixelsInput);
        console.log("Success.");
        return true;
    } catch (error) {
        console.error(error);
        return false;
    }
}
