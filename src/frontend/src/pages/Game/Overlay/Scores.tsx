import { FC, useMemo, useState } from "react";
import Guild from "../../../models/enum/Guild";
import { useGameStateStore } from "../../../stores/gameStateStore";
import "./overlay.css";
import { Score } from "./Score";

const topXScores = 3;
const showLabel = "View All";
const hideLabel = "Hide";

const Scores: FC = () => {
    const [showAll, setShowAll] = useState(false);
    const scores = useGameStateStore((state) => state.scores);

    const scoreElements = useMemo(() => {
        const sortedScores = scores.sort((a, b) => b.amount - a.amount);
        const visibleScores = showAll ? sortedScores : sortedScores.slice(0, topXScores);
        const result = visibleScores.map((score) => {
            const guild = score.guild;
            const guildName = Guild[guild];
            return <Score key={guildName} guild={guildName} score={score.amount} />;
        });
        return result;
    }, [scores, showAll]);

    return (
        <div className="top-overlay__left">
            <h2 className="top-overlay__score-title">Ticking scores:</h2>
            <div className="top-overlay__scores-container">{scoreElements}</div>
            <button className="top-overlay__score-button" onClick={() => setShowAll(prev => !prev)}>
                {showAll ? hideLabel : showLabel}
                </button>
        </div>
    );
};

export { Scores };
