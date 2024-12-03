import { FC, useMemo } from "react";
import Guild from "../../../models/enum/Guild";
import { useGameStateStore } from "../../../stores/gameStateStore";
import "./overlay.css";
import { Score } from "./Score";

const Scores: FC = () => {
    const scores = useGameStateStore((state) => state.scores);

    const scoreElements = useMemo(() => {
        const sortedScores = scores.sort((a, b) => b.amount - a.amount);
        const result = sortedScores.map((score) => {
            const guild = score.guild as Guild;
            const guildName = Guild[guild];
            return <Score key={guildName} guild={guildName} score={score.amount} />;
        });
        return result;
    }, [scores]);

    return <div className="top-overlay__left">{scoreElements}</div>;
};

export { Scores };
