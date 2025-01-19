import "./overlay.css";
import { SpecialEffect } from "./specialEffect";
import { useGameStateStore } from "../../../stores/gameStateStore";
import { usePowerUpStore } from "../../../stores/powerupStore";

const BottomOverlay = () => {
    const powerUp = usePowerUpStore(state => state.powerUp);
    const setPowerUp = usePowerUpStore(state => state.setPowerUp);
    const specialEffects = useGameStateStore(state => state.powerUps);

    return (
        <div className="bottom-overlay bottom-gradient">
            {specialEffects.map((effect, index) => <SpecialEffect key={`${effect.id}-${index}`} {...effect} selected={powerUp} onClick={setPowerUp} />)}
        </div>
    );
};

export { BottomOverlay };
