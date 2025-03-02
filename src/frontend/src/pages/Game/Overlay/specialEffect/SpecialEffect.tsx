import { FC, useMemo } from "react";
import "./style.css";
import defaultEffect from "../../../../assets/special_effect.png";
import { PowerUp } from "../../../../models/Get/GetGameState";
//TODO nää vois olla massa import ja filenamella variable
import titeeniKirvesEffect from "../../../../assets/powerups/powerup-axe.png";
import ruusuEffect2 from "../../../../assets/powerups/powerup-ruusu-2.png";
import isoLEffect from "../../../../assets/powerups/powerup-L.png";
import mFilesEffect from "../../../../assets/powerups/powerup-mfiles.png";
import siikaEffect from "../../../../assets/powerups/powerup-siika.png";
import binaryEffect from "../../../../assets/powerups/powerup-binary.png";

type Props = {
    selected: number | null;
    onClick: (effectId: number) => void;
};

const SpecialEffectIcon: { [key: number]: string } = {
    1: defaultEffect, 
    0: titeeniKirvesEffect,
    2: mFilesEffect, 
    3: isoLEffect, 
    4: ruusuEffect2, 
    5: binaryEffect,
    6: siikaEffect,
};

const getEffectIcon = (index: number) => {
    return SpecialEffectIcon[index] ?? SpecialEffectIcon[1];
};

export const SpecialEffect: FC<PowerUp & Props> = ({ selected, powerUpId, name, onClick }) => {
    const isSelected = useMemo(() => selected === powerUpId, [powerUpId, selected]);
    const icon = useMemo(() => getEffectIcon(powerUpId), [powerUpId]);
    console.log(powerUpId);
    return (
        <div key={powerUpId} className={"special-effect"} onClick={() => onClick(powerUpId)}>
            <div className={`button ${isSelected ? "selected" : ""}`}>
                <img className="icon" src={icon} />
            </div>
            <span className="label">{name}</span>
            {isSelected ? <img className="overlay-effect" src={icon} /> : null}
        </div>
    );
};
