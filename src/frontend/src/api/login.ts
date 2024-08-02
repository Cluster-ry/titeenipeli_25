import { PostLoginInput } from "../models/PostLoginInput";
import axios, { AxiosHeaders } from "axios";

const LOGIN_URL = "http://localhost:8080/login";


/**
 * @brief POST request for logging in
 * 
 * @param postLoginInput Username and Password 
 */
export async function postLogin(postLoginInput: PostLoginInput) {        
    try {            
        const headers = new AxiosHeaders();
        return await axios.post<PostLoginInput>(
            LOGIN_URL,
            postLoginInput,
            {
                headers,
                withCredentials: true
            }
        )
        .then(response => {
            console.log(response)       // REMOVE THIS
            if (response.status === 200) {
                console.log("Login successful.");
                return;
            }
            console.log("Login unsuccessful.");
        });
    } catch(error) {
        console.error(error);
    }
}