import { api } from "../api/index";

import { PostUsersInput } from "../models/PostUsersInput";
import { PostLoginInput } from "../models/PostLoginInput";
import { PostCtfInput } from "../models/PostCtfInput";
import { PostPixelsInput } from "../models/PostPixelsInput";
import { PutUsersInput } from "../models/PutUsersInput";

export default function ApiTestClient() {

    const logInTest = async (event: { preventDefault: () => void; }) => {
        event.preventDefault();

        const postUserInput: PostLoginInput = {
            Username: "test",
            Password: "test123"
        }

        api.login.postLogin(postUserInput);
    }
    
    const postUsersTest = async(event: { preventDefault: () => void; }) => {
        event.preventDefault();

        const postUserInput: PostUsersInput = {
            Id: "string",
            FirstName: "string",
            LastName: "string",
            Username: "string",
            PhotoUrl: "string",
            AuthDate: "string",
            Hash: "string"
        }
        api.users.postUsers(postUserInput);
    }

    const putUsersTest = async(event: { preventDefault: () => void; }) => {
        event.preventDefault();

        const putUsersInput: PutUsersInput = {
            Guild: "Tietokilta"
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
            X: -1,
            Y: 1
        }
        api.map.postPixels(postPixelsInput);
    }

    const postCtfTest = async(event: { preventDefault: () => void; }) => {
        event.preventDefault();
        const postCtfInput: PostCtfInput = {
            Token: "FGSTLBGXM3YB7USWS28KE2JV9Z267L48"
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