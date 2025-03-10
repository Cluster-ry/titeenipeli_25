import { FC, useCallback, useEffect, useMemo, useState } from "react";
import "./style.css";
import { PowerUp } from "../../../../models/Get/GetGameState";
import { imageAssets } from "../../../../assets/powerups";

type Props = {
    selected: number | null;
    onClick: (effectId: number) => void;
};

const cancelLabel = "Cancel";

const SpecialEffectIcon: { [key: number]: () => Promise<typeof import("*.png")> } = {
    1: imageAssets.defaultPowerup, 
    0: imageAssets.axePowerup,
    2: imageAssets.mFilesPowerup, 
    3: imageAssets.lPowerup, 
    4: imageAssets.ruusuPowerup, 
    5: imageAssets.binaryPowerup,
    6: imageAssets.siikaPowerup,
};

const getEffectIcon = async (index: number) => {
    const imageModule = await SpecialEffectIcon[index]?.();
    return imageModule?.default ?? (await (SpecialEffectIcon[1])?.())?.default ?? null;
};

export const SpecialEffect: FC<PowerUp & Props> = ({ selected, powerUpId, name, onClick }) => {
    const isSelected = useMemo(() => selected === powerUpId, [powerUpId, selected]);
    const [icon, setIcon] = useState<string | null>(null);

    useEffect(() => {
        const setEffectIcon = async () => {
            const icon = await getEffectIcon(powerUpId);
            setIcon(icon);
        }
        setEffectIcon();
    }, [powerUpId]);

    const handleClick = useCallback(() => {
        onClick(powerUpId);
    }, [onClick, powerUpId]);

    return (
        <div key={powerUpId} className="special-effect" onClick={handleClick}>
            <div className={`button ${isSelected ? "selected" : ""}`}>
                {icon ? <img className="icon low-res" src={icon} loading="lazy" /> : null}
            </div>
            <span className={`label${isSelected ? " cancel" : ""}`}>{isSelected ? cancelLabel : name}</span>
            {(isSelected && icon) ? <img className="overlay-effect low-res" src={icon} loading="lazy" /> : null}
        </div>
    );
};
