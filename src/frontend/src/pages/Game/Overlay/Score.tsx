import "./overlay.css";

type ScoreProps = {
    guild: string;
    score: number;
    position: number;
};

const Score = ({ guild, score, position }: ScoreProps) => {
    return (
        <div key={guild} className="top-overlay__score-container">
            <span>{guild.at(0)}</span>
            <span>{position}.</span>
            <span>{score}</span>
        </div>
    );
};

export { Score };
