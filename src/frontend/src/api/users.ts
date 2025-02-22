import axios, { AxiosResponse } from "axios";
import { PostUsersAuthenticateInput } from "../models/Post/PostUsersAuthenticateInput";
import { GetCurrentUserResult } from "../models/Get/GetCurrentUserResult.ts";

const USERS_URL = "api/v1/users";

export async function postUsersAuthenticate(postUsersInput: PostUsersAuthenticateInput): Promise<AxiosResponse> {
    const response = await axios.post(USERS_URL + "/authenticate", postUsersInput);

    return response;
}

export async function getCurrentUser() {
    return await axios.get<GetCurrentUserResult>(USERS_URL + "/current");
}
