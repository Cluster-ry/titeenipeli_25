import { api } from "../api/index";

import { PostUsersInput } from "../models/PostUsersInput";
import { PostLoginInput } from "../models/PostLoginInput";

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

    return (
        <>
            <form onSubmit={logInTest}>
                <input type="submit" value="Test Post Login"></input>
            </form>
            <form onSubmit={postUsersTest}>
                <input type="submit" value="Test Post Users"></input>
            </form>
            <form>

            </form>
            <form>

            </form>
        </>
    )
}