import { FC } from "react";
import "./overlay.css";

type ScoreProps = {
    guild: string;
    score: number;
};

const Score: FC<ScoreProps> = ({ guild, score }) => {
    return (
        <div key={guild} className="top-overlay__score-container">
            <span>{guild}:</span>
            <span>{score}</span>
        </div>
    );
};

export { Score };
