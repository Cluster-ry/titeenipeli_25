import axios, { AxiosHeaders } from "axios";
import { PostPixelsInput } from "../models/PostPixelsInput";

const PIXELS_URL = "/map/pixels";

export async function getPixels() {
    try {
        const headers = new AxiosHeaders();
        return await axios.get<PostPixelsInput>(
            PIXELS_URL,
            {
                headers
            }
        )
        .then ( response => {
            console.log(response);      // REMOVE THIS
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

export async function postPixels(postPixelsInput: PostPixelsInput) {
    const headers = new AxiosHeaders();
    try {
        return axios.post<PostPixelsInput>(
            PIXELS_URL,
            postPixelsInput,
            {
                headers
            }
        )
        .then( response => {
            console.log(response);      // REMOVE THIS
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