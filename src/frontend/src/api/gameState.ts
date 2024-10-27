import axios, { AxiosResponse } from "axios";
import { GetPixelsResult } from "../models/Get/GetPixelsResult";
import { ClientApiError } from "../models/ClientApiError";

const PIXELS_URL = "api/v1/state/game";

export async function getGameState(): Promise<AxiosResponse<GetPixelsResult> | ClientApiError> {
    try {
        const response = await axios.get<GetPixelsResult>(PIXELS_URL);

        console.log("Success.");
        return response;
    } catch (error) {
        console.error(error);
        return { msg: "Request unsuccessful." };
    }
}
