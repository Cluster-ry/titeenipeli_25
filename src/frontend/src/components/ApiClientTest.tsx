/**
 * This file contains a bunch of tests on the client side. The purpose 
 * of these tests is to make sure the APIs work so that the 
 * communication with the back-end is functional.
 *
 * Contains the following test:
 * 1) POST request for logging in
 * 2) POST request for /api/v1/users
 * 3) PUT request for /api/v1/users 
 * 4) GET request for /api/v1/map/pixels
 * 5) POST request for /api/v1/map/pixels
 * 6) POST request for /ctf
 */
import { api } from "../api/index";

import { PostUsersInput } from "../models/Post/PostUsersInput";
import { PostLoginInput } from "../models/Post/PostLoginInput";
import { PostCtfInput } from "../models/Post/PostCtfInput";
import { PostPixelsInput } from "../models/Post/PostPixelsInput";
import { PutUsersInput } from "../models/Put/PutUsersInput";

const ApiTestClient = () => {

    const logInTest = async (event: { preventDefault: () => void; }) => {
        event.preventDefault();

        const postUserInput: PostLoginInput = {
            username: "test",
            password: "test123"
        }

        api.login.postLogin(postUserInput);
    }
    
    const postUsersTest = async(event: { preventDefault: () => void; }) => {
        event.preventDefault();

        const postUserInput: PostUsersInput = {
            id: "test",
            firstName: "string",
            lastName: "string",
            username: "string",
            photoUrl: "string",
            authDate: "string",
            hash: "string"
        }
        api.users.postUsers(postUserInput);
    }

    const putUsersTest = async(event: { preventDefault: () => void; }) => {
        event.preventDefault();

        const putUsersInput: PutUsersInput = {
            guild: "Tietokilta"
        }
        api.users.putUsers(putUsersInput);
    }

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
            <form onSubmit={logInTest} style={{marginBottom: "2vh"}}>
                <input type="submit" value="Test Post Login"></input>
            </form>
            <form onSubmit={postUsersTest} style={{marginBottom: "2vh"}}>
                <input type="submit" value="Test Post Users"></input>
            </form>
            <form onSubmit={putUsersTest} style={{marginBottom: "2vh"}}>
                <input type="submit" value="Test Put Users"></input>
            </form>
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
