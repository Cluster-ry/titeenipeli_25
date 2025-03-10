import { useEffect, useState } from "react";
import { api } from "../api/index";
import { GetScores } from "../models/Get/GetScores";
import { shortGuildName } from "../models/enum/Guild";

export const Scores = () => {
    const [currentScores, setCurrentScores] = useState("");

    useEffect(() => {
        getScores();
        setInterval(getScores, 15000);
    }, []);

    const getScores = async () => {
        const data = (await api.scores.getScores()).data as GetScores;
        const scores = data.scores;

        scores.sort((a, b) => b.score - a.score);

        let scoresText = "";
        for (const score of scores) {
            const guildName = shortGuildName(score.guild);
            const guildSpaces = " ".repeat(10 - guildName.length);

            scoresText += `${shortGuildName(score.guild)}:${guildSpaces}${score.score}\n`;
        }
        setCurrentScores(scoresText);
    };

    return (
        <p
            style={{
                whiteSpace: "pre-wrap",
                textAlign: "left",
                marginLeft: "2em",
            }}
        >
            {currentScores}
        </p>
    );
};

export default Scores;
