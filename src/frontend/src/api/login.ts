import axios, { AxiosResponse } from "axios";
import { PostLoginInput } from "../models/Post/PostLoginInput";
import { ClientApiError } from "../models/ClientApiError";
import { ClientApiOk } from "../models/ClientApiOk";

const LOGIN_URL = "api/v1/users";


/**
 * @brief POST request for logging in
 * 
 * @param postLoginInput Username and Password 
 */
export async function postLogin(postLoginInput: PostLoginInput): Promise<AxiosResponse<ClientApiOk> | ClientApiError> {        
    try {            
        const response = await axios.post<ClientApiOk>(
            LOGIN_URL,
            postLoginInput,
            {
                withCredentials: true
            }
        );
        
        console.log("Login successful.");
        return response;
    } catch(error) {
        console.error(error);
        return { msg: "Request unsuccessful." };
    }
}