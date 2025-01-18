import { FC, useMemo } from "react";
import Guild from "../../../models/enum/Guild";
import { useGameStateStore } from "../../../stores/gameStateStore";
import "./overlay.css";
import { Score } from "./Score";

const topXScores = 3;

const Scores: FC = () => {
    const scores = useGameStateStore((state) => state.scores);

    const scoreElements = useMemo(() => {
        const sortedScores = scores.sort((a, b) => b.amount - a.amount);
        const splicedScores = sortedScores.slice(0, topXScores);
        const result = splicedScores.map((score) => {
            const guild = score.guild as Guild;
            const guildName = Guild[guild];
            return <Score key={guildName} guild={guildName} score={score.amount} />;
        });
        return result;
    }, [scores]);

    return (
        <div className="top-overlay__left">
            <h2 className="top-overlay__score-title">Ticking scores:</h2>
            <div className="top-overlay__scores-container">{scoreElements}</div>
            <button className="top-overlay__score-button">View all</button>
        </div>
    );
};

export { Scores };
