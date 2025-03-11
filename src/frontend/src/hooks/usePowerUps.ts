import { activatePowerUp } from "../api/powerup";
import { Coordinate } from "../models/Coordinate";
import { Direction } from "../models/enum/PowerUp";
import { PostPowerup } from "../models/Post/PostPowerup";
import { useNotificationStore } from "../stores/notificationStore";
import { usePowerUpStore } from "../stores/powerupStore";

const invalidActivationLabel = "Invalid powerup activation. Select an adjacent tile as the starting point";

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
    const setTarget = usePowerUpStore((state) => state.setTarget);
    const resetPowerUp = usePowerUpStore((state) => state.resetPowerUp);
    const triggerNotification = useNotificationStore(state => state.triggerNotification);
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
                const success = await activatePowerUp(props);
                if (success) {
                    resetPowerUp();
                } else {
                    triggerNotification(invalidActivationLabel, "neutral");
                }
            };
            activate({ id: powerUp, location: target, direction });
        }
        return true;
    };
    return usePowerUp;
};
