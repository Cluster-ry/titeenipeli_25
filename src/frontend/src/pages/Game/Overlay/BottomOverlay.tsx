import "./overlay.css";
import { SpecialEffect } from "./specialEffect";
import { useGameStateStore } from "../../../stores/gameStateStore";
import { usePowerUpStore } from "../../../stores/powerupStore";

import BottomOverlayRight from "./BottomOverlayRight";
import { useMemo } from "react";

const BottomOverlay = () => {
    const specialEffects = useGameStateStore((state) => state.powerUps);
    const powerUp = usePowerUpStore((state) => state.powerUp);
    const setPowerUp = usePowerUpStore((state) => state.setPowerUp);
    const uiPowerUpId  = usePowerUpStore((state) => state.uiPowerUpId);
    
  const specialEffectItems = useMemo(() =>
    specialEffects.map((effect, index) => (
    <SpecialEffect
        key={`${effect.powerUpId}-${index}`}
        {...effect}
        selected={powerUp}
        onClick={setPowerUp}
        keyAsId={`${effect.powerUpId}-${index}`}
        uiPowerUpId={uiPowerUpId}
        />
    )),
    [specialEffects, powerUp, setPowerUp]);

  return (
    <div className="bottom-overlay">
      <div className="bottom-overlay__left">
        {specialEffectItems}
      </div>
      <BottomOverlayRight />
    </div>
  );
};

export { BottomOverlay };
