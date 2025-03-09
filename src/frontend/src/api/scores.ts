import axios from "axios";
import { GetScores } from "../models/Get/GetScores";

const SCORES_URL = "api/v1/result/score";

export async function getScores() {
    return axios.get<GetScores>(SCORES_URL);
}
