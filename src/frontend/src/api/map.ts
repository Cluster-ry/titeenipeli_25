import axios from "axios";
import { PostPixelsInput } from "../models/PostPixelsInput";

const PIXELS_URL = "http://localhost:8080/map/pixels";

export async function getPixels() {
    try {
        return await axios.get<PostPixelsInput>(
            PIXELS_URL,
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
    try {
        return axios.post<PostPixelsInput>(
            PIXELS_URL,
            postPixelsInput,
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