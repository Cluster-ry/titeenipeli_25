import axios from "axios";
import { PostLoginInput } from "../models/PostLoginInput";
import { PostCtfInput } from "../models/PostCtfInput";
import { PostPixelsInput } from "../models/PostPixelsInput";

/**
 * @brief Sending a POST request for logging the user in. 
 * @param postLoginInput Username and Password 
 */
function postLogin(postLoginInput: PostLoginInput) {
    return axios.post<PostLoginInput>(
        "/login",
        postLoginInput,
        {
            headers: {
                "Content-Type": "application/json"
            },
            withCredentials: true
        }
    )
    // Inspecting the response
    // Currently only checks for success
    .then(response => {
        console.log(response)
        if (response.status === 200) {
            console.log("Login successful.");
        } else {
            console.log("Login unsuccessful.");
        }
    })

    // Catching the errors.
    // Currently not an effective implementation.
    .catch(error => {
        console.error("Encountered an error", error);
        throw error;
    });
}

/**
 * @brief 
 * @param postCtfInput 
 */
function postCtf(postCtfInput: PostCtfInput) {
    return axios.post<PostCtfInput>(
        "/ctf",
        postCtfInput,
        {
            headers: {
                "Content-Type": "application/json"
            }
        }
    )
    .then( response => {
    })
    ;
}

function getPixels() {
    return axios.get<PostPixelsInput>(
        "/map/pixels",
        {
            headers: {
                "Content-Type": "application/json"
            }
        }
    );
}

function postPixels(postPixelsInput: PostPixelsInput) {
    return axios.post<PostPixelsInput>(
        "/map/pixels",
        postPixelsInput,
        {
            headers: {
                "Content-Type": "application/json"
            }
        }
    );
}

export { postLogin, postCtf, postPixels, getPixels };