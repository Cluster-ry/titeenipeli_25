import Guild from "../enum/Guild";

export interface GetScores {
    scores: Score[];
}

export interface Score {
    guild: Guild;
    score: number;
}
