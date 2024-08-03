import axios, { AxiosResponse } from "axios";
import { PostUsersInput } from "../models/PostUsersInput";
import { PutUsersInput } from "../models/PutUsersInput";
import { ClientApiError } from "../models/ClientApiError";

const USERS_URL = "http://localhost:8080/users";

export async function postUsers(postUsersInput: PostUsersInput): Promise<AxiosResponse<PostUsersInput> | ClientApiError> {
    try {
        const response = await axios.post<PostUsersInput>(
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

export async function putUsers(putUsersInput: PutUsersInput): Promise<AxiosResponse<PutUsersInput> | ClientApiError> {
    try {
        const response = await axios.put<PutUsersInput>(
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