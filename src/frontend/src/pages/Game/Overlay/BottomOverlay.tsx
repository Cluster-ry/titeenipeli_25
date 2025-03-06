import "./overlay.css";
import { SpecialEffect } from "./specialEffect";
import { useGameStateStore } from "../../../stores/gameStateStore";
import { usePowerUpStore } from "../../../stores/powerupStore";
import powerupIcon from "../../../assets/sprites/powerup.png";

import BottomOverlayRight from "./BottomOverlayRight";
import { useState } from "react";

const BottomOverlay = () => {
    const specialEffects = useGameStateStore((state) => state.powerUps);
    const powerUp = usePowerUpStore((state) => state.powerUp);
    const setPowerUp = usePowerUpStore((state) => state.setPowerUp);
  const [powerupVisible, setPowerupVisible] = useState(false);

  return (
    <div className="bottom-overlay bottom-gradient">

      <div className="bottom-overlay__left">
      {
        powerupVisible 
        ? 
        specialEffects.map((effect, index) => (
        <SpecialEffect
          key={`${effect.powerUpId}-${index}`}
          {...effect}
          selected={powerUp}
          onClick={setPowerUp}
        />
        ))
        : <></>
      
      }

      </div>
      <div className="bottom-overlay__middle">
        <div
          className="bottom-overlay__imgwrapper"
          onClick={() => {setPowerupVisible(!powerupVisible)}}>
        <img className="bottom-overlay__middle__powerup" src={powerupIcon} />
        </div>
      </div>
      <BottomOverlayRight />
    </div>
  );
};

export { BottomOverlay };
