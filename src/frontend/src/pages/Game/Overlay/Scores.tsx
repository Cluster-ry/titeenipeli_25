import Guild from "../../../models/enum/Guild";
import { useGameStateStore } from "../../../stores/gameStateStore";
import "./overlay.css";
import { Score } from "./Score";

const Scores = () => {
    const scores = useGameStateStore((state) => state.scores);
    const sortedScores = scores.sort((a, b) => b.amount - a.amount);

    const scoreElements = sortedScores.map((score) => {
        const guild = score.guild as Guild;
        const guildName = Guild[guild];

        return <Score guild={guildName} score={score.amount} />;
    });

    return <div className="top-overlay__left">{scoreElements}</div>;
};

export { Scores };
