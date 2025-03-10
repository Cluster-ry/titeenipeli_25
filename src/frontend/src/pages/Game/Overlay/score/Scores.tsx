import { FC, useCallback, useMemo, useState } from "react";
import { shortGuildName } from "../../../../models/enum/Guild";
import { useGameStateStore } from "../../../../stores/gameStateStore";
import "../overlay.css";
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
        const result = visibleScores.map((score, index) => {
            const guild = score.guild;
            const guildName = shortGuildName(guild);
            return <Score key={guildName} guild={guildName} score={score.amount} place={index} />;
        });
        return result;
    }, [scores, showAll]);

    const toggleShowAll = useCallback(() => {
        setShowAll((prev) => !prev);
    }, [setShowAll]);

    return (
        <div className="top-overlay__left outlined-text">
            <div className="top-overlay__scores-container">{scoreElements}</div>
            <button className="top-overlay__score-button" onClick={toggleShowAll}>
                {showAll ? hideLabel : showLabel}
            </button>
        </div>
    );
};

export { Scores };
