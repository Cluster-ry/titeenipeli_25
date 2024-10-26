import axios, { AxiosResponse } from "axios";
import { PostUsersAuthenticateInput } from "../models/Post/PostUsersAuthenticateInput";

const USERS_URL = "api/v1/users";

export async function postUsersAuthenticate(postUsersInput: PostUsersAuthenticateInput): Promise<AxiosResponse> {
    const response = await axios.post(USERS_URL + "/authenticate", postUsersInput);

    return response;
}
