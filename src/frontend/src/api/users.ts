import axios, { AxiosHeaders } from "axios";
import { PostUsersInput } from "../models/PostUsersInput";

const USERS_URL = "http://localhost:8080/users";

export async function postUsers(postUsersInput: PostUsersInput) {
    try {
        const headers = new AxiosHeaders();
        return await axios.post<PostUsersInput>(
            USERS_URL,
            postUsersInput,
            {
                headers,
            }
        )
        .then(response => {
            console.log(response)       // REMOVE THIS
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