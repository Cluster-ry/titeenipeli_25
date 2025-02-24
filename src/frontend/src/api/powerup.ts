import axios from "axios";
import { PostPowerup } from "../models/Post/PostPowerup";
import { ClientApiOk } from "../models/ClientApiOk";

const POWERUPS_URL: string = "api/v1/state/powerups";

export const activatePowerUp = async (powerup: PostPowerup) => {
    try {
        await axios.post<ClientApiOk>(`${POWERUPS_URL}/activate`, powerup);
        console.debug("Success.");
        return true;
    } catch (err) {
        console.error(`Failed to activate powerup ${powerup.id}\n${err}`);
        return false;
    }
};
