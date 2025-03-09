import { FC, useMemo } from "react";
import "../overlay.css";

import goldMedal from "../../../../assets/sprites/medal-gold.png";
import silverMedal from "../../../../assets/sprites/medal-silver.png";
import bronzeMedal from "../../../../assets/sprites/medal-bronze.png";

type ScoreProps = {
    guild: string;
    score: number;
    place: number;
};

const medals = [goldMedal, silverMedal, bronzeMedal];

const Score: FC<ScoreProps> = ({ guild, score, place }) => {

    const medal = useMemo(() => {
        if (place <= medals.length) {
            return <img src={medals[place]} />
        }
        return null;
    }, [place]);

    return (
        <div key={guild} className="top-overlay__score-container">
            {medal}
            <span>{guild}:</span>
            <span>{score}</span>
        </div>
    );
};

export { Score };
