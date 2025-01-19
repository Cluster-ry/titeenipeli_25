import { useCallback, useState } from "react";
import PowerUp from "../../../models/PowerUp";
import "./overlay.css";
import { SpecialEffect } from "./specialEffect";

const BottomOverlay = () => {
    const [selected, setSelected] = useState<number | null>(null);
    // Placeholders
    const specialEffects: PowerUp[] = [
        { Id: 1, Name: "Titeeni-\nkirves", Description: "Jeejee", Directed: true },
        { Id: 2, Name: "Pyssy saatana", Description: "Juujuu", Directed: false },
        { Id: 3, Name: "Taktinen anustappi", Description: "Jiijee", Directed: false },
    ];
    const onClick = useCallback(
        (id: number) => {
            setSelected((prev) => (prev !== id ? id : null));
        },
        [setSelected],
    );
    return (
        <div className="bottom-overlay bottom-gradient">
            {specialEffects.map((effect) => (
                <SpecialEffect key={effect.Id} {...effect} selected={selected} onClick={onClick} />
            ))}
        </div>
    );
};

export { BottomOverlay };
