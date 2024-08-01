import axios, { AxiosHeaders } from "axios";
import { PostLoginInput } from "../models/PostLoginInput";
import { PostCtfInput } from "../models/PostCtfInput";
import { PostPixelsInput } from "../models/PostPixelsInput";


/**
 * This file contains the front-end API client functionality. Every function
 * is conveniently wrapped inside a namespace. 
 * 
 * @Note
 * This file is currently still not finished, thus it contains
 * console.log commands that should not exist when the application
 * is pushed into production.
 */

const LOGIN_URL     = "/login";
const CTF_URL       = "/ctf";
const PIXELS_URL    = "/map/pixels";

namespace ApiClient {

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
    
    /**
     * @param postCtfInput 
     */
    export async function postCtf(postCtfInput: PostCtfInput) {
        try {
            const headers = new AxiosHeaders();
            return await axios.post<PostCtfInput>(
                CTF_URL,
                postCtfInput,
                {
                    headers
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
        } catch(error) {
            console.error(error);
        }
    }
    
    export async function getPixels() {
        try {
            const headers = new AxiosHeaders();
            return await axios.get<PostPixelsInput>(
                PIXELS_URL,
                {
                    headers
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
        } catch(error) {
            console.error(error);
        }
    }
    
    export async function postPixels(postPixelsInput: PostPixelsInput) {
        const headers = new AxiosHeaders();
        try {
            return axios.post<PostPixelsInput>(
                PIXELS_URL,
                postPixelsInput,
                {
                    headers
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
        } catch(error) {
            console.error(error);
        }
    }
}

export { ApiClient }