import { postCtf, postLogin, postPixels, getPixels } from "../api/api"
import { PostLoginInput } from "../models/PostLoginInput";

export default function ApiTestClient() {


    const logInTest = async (event: any) => {
        event.preventDefault();

        const testUser: PostLoginInput = {
            Username: "test",
            Password: "test123"
        }

        await postLogin(testUser);
    }
    return (
        <>
            <form onSubmit={logInTest}>

            </form>
        </>
    )
}