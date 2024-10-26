/**
 * This file contains a bunch of tests on the client side. The purpose 
 * of these tests is to make sure the APIs work so that the 
 * communication with the back-end is functional.
 */
import { api } from "../api/index";

import { PostCtfInput } from "../models/Post/PostCtfInput";
import { PostPixelsInput } from "../models/Post/PostPixelsInput";


export function ApiTestClient() {
    const getMapTest = async(event: { preventDefault: () => void; }) => {
        event.preventDefault();
        api.map.getPixels();
    }

    const postMapTest = async(event: { preventDefault: () => void; }) => {
        event.preventDefault();
        const postPixelsInput: PostPixelsInput = {
            x: -1,
            y: 1
        }
        api.map.postPixels(postPixelsInput);
    }

    const postCtfTest = async(event: { preventDefault: () => void; }) => {
        event.preventDefault();
        const postCtfInput: PostCtfInput = {
            token: "FGSTLBGXM3YB7USWS28KE2JV9Z267L48"
        }
        api.ctf.postCtf(postCtfInput);
    }

    return (
        <>
            <form onSubmit={getMapTest} style={{marginBottom: "2vh"}}>
                <input type="submit" value="Test Get Map"></input>
            </form>
            <form onSubmit={postMapTest} style={{marginBottom: "2vh"}}>
                <input type="submit" value="Test Post Map"></input>
            </form>
            <form onSubmit={postCtfTest} style={{marginBottom: "2vh"}}>
                <input type="submit" value="Test Post Ctf"></input>
            </form>
        </>
    )
}

export default ApiTestClient;
