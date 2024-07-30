import axios from "axios";
import { PostLoginInput } from "../models/PostLoginInput";
import { PostCtfInput } from "../models/PostCtfInput";
import { PostPixelsInput } from "../models/PostPixelsInput";

/**
 * @NOTE 
 * 
 * The file currently has console.logs to help inspect
 * behavior. These should be removed when the application
 * is put to production.
 */


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
        console.log(response)       // REMOVE THIS
        if (response.status === 200) {
            console.log("Login successful.");
            return;
        }
        console.log("Login unsuccessful.");
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
        console.log(response)       // REMOVE THIS
        if (response.status === 200) {
            console.log("Success.");
            return;
        }
        console.log("Failure.");
    });
}

function getPixels() {
    return axios.get<PostPixelsInput>(
        "/map/pixels",
        {
            headers: {
                "Content-Type": "application/json"
            }
        }
    )
    .then ( response => {
        console.log(response);      // REMOVE THIS
        if (response.status === 200) {
            console.log("Success.");
            return;
        }
        console.log("Failure.");
    });
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
    )
    .then( response => {
        console.log(response);      // REMOVE THIS
        if (response.status === 200) {
            console.log("Success.");
            return;
        }
        console.log("Failure.");
    });
}

export { postLogin, postCtf, postPixels, getPixels };