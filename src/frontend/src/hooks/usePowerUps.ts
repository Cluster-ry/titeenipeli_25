import { activatePowerUp } from "../api/powerup";
import { Coordinate } from "../models/Coordinate";
import { Direction } from "../models/enum/PowerUp";
import { PostPowerup } from "../models/Post/PostPowerup";
import { usePowerUpStore } from "../stores/powerupStore";

const getDirection = (center: Coordinate, target: Coordinate) => {
    const directionX = center.x - target.x;
    const directionY = center.y - target.y;
    if (directionX === 0 && directionY === 0) return Direction.None;
    const direction =
        Math.abs(directionX) > Math.abs(directionY)
            ? directionX > 0
                ? Direction.West
                : Direction.East
            : directionY > 0
              ? Direction.North
              : Direction.South;
    return direction;
};

export const usePowerUps = () => {
    const setTarget = usePowerUpStore(state => state.setTarget);
    const resetPowerUp = usePowerUpStore(state => state.resetPowerUp);
    // Returns whether event propagation should be stopped
    const usePowerUp = (coordinate: Coordinate, targeted: boolean) => {
        // Again, for some god knows what reason the state just won't stay up to date without getting it manually
        const { powerUp, target } = usePowerUpStore.getState();
        if (powerUp === null) return false;
        if (target === null) {
            setTarget(coordinate);
            return true;
        }
        if (targeted) {
            const direction = getDirection(target, coordinate);
            if (direction === 0) return true;
            const activate = async (props: PostPowerup) => {
                await activatePowerUp(props);
            };
            activate({ id: powerUp, location: target, direction });
            resetPowerUp();
        }
        return true;
    };
    return { usePowerUp };
};
