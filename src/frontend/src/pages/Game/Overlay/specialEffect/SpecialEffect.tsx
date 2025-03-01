import { FC, useMemo } from "react";
import "./style.css";
import defaultEffect from "../../../../assets/special_effect.png";
import { PowerUp } from "../../../../models/Get/GetGameState";
import titeeniKirvesEffect from "../../../../assets/powerup-axe.png";
import mFilesEffect from "../../../../assets/powerup-mfiles.png";

type Props = {
    selected: number | null;
    onClick: (effectId: number) => void;
};

const SpecialEffectIcon: { [key: number]: string } = {
    99: defaultEffect, //TODO default should be what?
    0: titeeniKirvesEffect,
    1: mFilesEffect, 
    //2: isoLEffect, //TODO ico missing
    //3: ruusuEffect, //TODO ico missing
    //4, binaryEffect //TODO ico missing
    //5: glitchEffect, //TOODO ico missing
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
