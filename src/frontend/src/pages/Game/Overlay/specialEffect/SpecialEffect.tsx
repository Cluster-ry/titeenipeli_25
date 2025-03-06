import { FC, useCallback, useMemo } from "react";
import "./style.css";
import defaultEffect from "../../../../assets/special_effect.png";
import { PowerUp } from "../../../../models/Get/GetGameState";

type Props = {
    selected: number | null;
    onClick: (effectId: number) => void;
};

const cancelLabel = "Cancel";

const SpecialEffectIcon: { [key: number]: string } = {
    1: defaultEffect,
};

const getEffectIcon = (index: number) => {
    return SpecialEffectIcon[index] ?? SpecialEffectIcon[1];
};

export const SpecialEffect: FC<PowerUp & Props> = ({ selected, powerUpId, name, onClick }) => {
    const isSelected = useMemo(() => selected === powerUpId, [powerUpId, selected]);
    const icon = useMemo(() => getEffectIcon(powerUpId), [powerUpId]);
    const handleClick = useCallback(() => {
        onClick(powerUpId);
    }, [onClick, powerUpId]);
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
