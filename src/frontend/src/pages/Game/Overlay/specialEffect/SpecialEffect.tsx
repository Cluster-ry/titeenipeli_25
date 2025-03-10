import { FC, useCallback, useMemo } from "react";
import "./style.css";
import defaultEffect from "../../../../assets/special_effect.png";
import { PowerUp } from "../../../../models/Get/GetGameState";
import titeeniKirvesEffect from "../../../../assets/powerups/powerup-axe.png";
import ruusuEffect2 from "../../../../assets/powerups/powerup-ruusu-2.png";
import isoLEffect from "../../../../assets/powerups/powerup-L.png";
import mFilesEffect from "../../../../assets/powerups/powerup-mfiles.png";
import siikaEffect from "../../../../assets/powerups/powerup-siika.png";
import binaryEffect from "../../../../assets/powerups/powerup-binary.png";
import starEffect from "../../../../assets/powerups/powerup-star.png";
//import bottleEffect from "../../../../assets/powerups/powerup-bottle.png";
import dinoEffect from "../../../../assets/powerups/powerup-dino.png";
import gemEffect from "../../../../assets/powerups/powerup-diamond.png";
import heartEffect from "../../../../assets/powerups/powerup-heart.png";
import spaceInvaderEffect from "../../../../assets/powerups/powerup-invader.png";

type Props = {
    selected: number | null;
    onClick: (effectId: number, keyAsId: string) => void;
    keyAsId: string;
    uiPowerUpId: string | null;
};

const cancelLabel = "Cancel";

const SpecialEffectIcon: { [key: number]: string } = {
    1: defaultEffect,
    0: titeeniKirvesEffect,
    2: mFilesEffect,
    3: isoLEffect,
    4: ruusuEffect2,
    5: binaryEffect,
    6: siikaEffect,
    8: starEffect,
    10: dinoEffect,
    11: gemEffect,
    12: heartEffect,
    13: spaceInvaderEffect
};
const getEffectIcon = (index: number) => {
    return SpecialEffectIcon[index] ?? SpecialEffectIcon[1];
};

export const SpecialEffect: FC<PowerUp & Props> = ({powerUpId, name, onClick, keyAsId, uiPowerUpId}) => {
    const isSelected = useMemo(() => uiPowerUpId === keyAsId, [keyAsId, uiPowerUpId]);
    const icon = useMemo(() => getEffectIcon(powerUpId), [powerUpId]);
    const handleClick = useCallback(() => {
        onClick(powerUpId, keyAsId);
    }, [onClick, powerUpId, keyAsId]);
    return (
        <div key={powerUpId} className={"special-effect"} onClick={handleClick}>
            <div className={`button ${isSelected ? "selected" : ""}`}>
                <img className="icon" src={icon} />
            </div>
            <span className={`label${isSelected ? " cancel" : ""}`}>{isSelected ? cancelLabel : name}</span>
            {isSelected ? <img className="overlay-effect" src={icon} /> : null}
        </div>
    );
};
