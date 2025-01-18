import { FC, useMemo } from "react";
import './style.css';
import defaultEffect from '../../../../assets/special_effect.png';
import PowerUp from "../../../../models/PowerUp";

type Props = {
    selected: number | null;
    onClick: (effectId: number) => void;
};

const SpecialEffectIcon: {[key: number]: string} = {
    1: defaultEffect,
}

const getEffectIcon = (index: number) => {
    return SpecialEffectIcon[index] ?? SpecialEffectIcon[1];
};

export const SpecialEffect: FC<PowerUp & Props> = ({ Id, Name, selected, onClick }) => {
    const isSelected = useMemo(() => selected === Id, [Id, selected]);
    const icon = useMemo(() => getEffectIcon(Id), [Id]);
    return (
        <div key={Id} className={"special-effect"} onClick={() => onClick(Id)}>
            <div className={`button ${isSelected ? "selected" : ""}`}>
                <img className="icon" src={icon} />
            </div>
            <span className="label">{Name}</span>
            {isSelected ? <img className="overlay-effect" src={icon} /> : null}
        </div>
    );
};