/**
 * This file contains a bunch of tests on the client side. The purpose
 * of these tests is to make sure the APIs work so that the
 * communication with the back-end is functional.
 */
import { AxiosResponse } from "axios";
import { api } from "../api/index";
import { GetPixelsResult } from "../models/Get/GetPixelsResult";

import { PostCtfInput } from "../models/Post/PostCtfInput";
import { PostPixelsInput } from "../models/Post/PostPixelsInput";

export function ApiTestClient() {
    const getMapTest = async (event: { preventDefault: () => void }) => {
        event.preventDefault();
        api.map.getPixels();
    };

    const postMapTest = async (event: { preventDefault: () => void }) => {
        event.preventDefault();
        const postPixelsInput: PostPixelsInput = {
            x: -1,
            y: 1,
        };
        api.map.postPixels(postPixelsInput);
    };

    const postCtfTest = async (event: { preventDefault: () => void }) => {
        event.preventDefault();
        const postCtfInput: PostCtfInput = {
            token: "FGSTLBGXM3YB7USWS28KE2JV9Z267L48",
        };
        api.ctf.postCtf(postCtfInput);
    };

    const getGameState = async (event: { preventDefault: () => void }) => {
        event.preventDefault();
        const results = (await api.gameState.getGameState()) as AxiosResponse<GetPixelsResult>;
        console.log(results.data);
    };

    return (
        <>
            <form onSubmit={getMapTest} style={{ marginBottom: "2vh" }}>
                <input type="submit" value="Test Get Map"></input>
            </form>
            <form onSubmit={postMapTest} style={{ marginBottom: "2vh" }}>
                <input type="submit" value="Test Post Map"></input>
            </form>
            <form onSubmit={postCtfTest} style={{ marginBottom: "2vh" }}>
                <input type="submit" value="Test Post Ctf"></input>
            </form>
            <form onSubmit={getGameState} style={{ marginBottom: "2vh" }}>
                <input type="submit" value="Test Get game state"></input>
            </form>
        </>
    );
}

export default ApiTestClient;
