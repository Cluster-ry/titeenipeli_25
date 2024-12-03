import axios, { AxiosResponse } from "axios";
import { ClientApiError } from "../models/ClientApiError";
import { GetGameState } from "../models/Get/GetGameState";

const PIXELS_URL = "api/v1/state/game";

export async function getGameState(): Promise<AxiosResponse<GetGameState> | ClientApiError> {
    try {
        const response = await axios.get<GetGameState>(PIXELS_URL);

        console.log("Success.");
        return response;
    } catch (error) {
        console.error(error);
        return { msg: "Request unsuccessful." };
    }
}
