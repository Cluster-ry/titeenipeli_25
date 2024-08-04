import axios, { AxiosResponse } from "axios";
import { PostUsersInput } from "../models/Post/PostUsersInput";
import { PutUsersInput } from "../models/Put/PutUsersInput";
import { ClientApiError } from "../models/ClientApiError";
import { PostUserResult } from "../models/Post/PostUserResult";
import { ClientApiOk } from "../models/ClientApiOk";

const USERS_URL = "api/v1/users";

export async function postUsers(postUsersInput: PostUsersInput): Promise<AxiosResponse<PostUserResult> | ClientApiError> {
    try {
        const response = await axios.post<PostUserResult>(
            USERS_URL,
            postUsersInput,
        );

        console.log("Success.");
        return response;

    } catch(error) {
        console.error(error);
        return { msg: "Request unsuccessful." };
    }
}

export async function putUsers(putUsersInput: PutUsersInput): Promise<AxiosResponse<ClientApiOk> | ClientApiError> {
    try {
        const response = await axios.put<ClientApiOk>(
            USERS_URL,
            putUsersInput
        );

        console.log("Success.");
        return response;
    } catch(error) {
        console.error(error);
        return { msg: "Request unsuccessful." };
    }
}