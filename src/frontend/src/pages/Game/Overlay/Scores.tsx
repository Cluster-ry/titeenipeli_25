import { FC, useMemo } from "react";
import { shortGuildName } from "../../../models/enum/Guild";
import { useGameStateStore } from "../../../stores/gameStateStore";
import "./overlay.css";
import { Score } from "./Score";

const topXScores = 3;

const Scores: FC = () => {
    const scores = useGameStateStore((state) => state.scores);

    const scoreElements = useMemo(() => {
        const sortedScores = scores.sort((a, b) => b.amount - a.amount);
        const visibleScores = sortedScores.slice(0, topXScores);
        const result = visibleScores.map((score, index) => {
            const guild = score.guild;
            const guildName = shortGuildName(guild);
            return <Score key={guildName} guild={guildName} score={score.amount} place={index}/>;
        });
        return result;
    }, [scores]);

    return (
        <div className="top-overlay__left">
            <div className="top-overlay__scores-container">{scoreElements}</div>
        </div>
    );
};

export { Scores };
