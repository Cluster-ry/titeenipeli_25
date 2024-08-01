import { api } from "../api/index";

import { PostLoginInput } from "../models/PostLoginInput";

export default function ApiTestClient() {


    const logInTest = async (event: { preventDefault: () => void; }) => {
        event.preventDefault();

        const testUser: PostLoginInput = {
            Username: "test",
            Password: "test123"
        }
        api.login.postLogin(testUser);
    }
    return (
        <>
            <form onSubmit={logInTest}>
                <input type="submit"></input>
            </form>
        </>
    )
}