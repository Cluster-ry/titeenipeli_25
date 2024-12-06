import Guild from "../enum/Guild";

export interface GetGameState {
    pixelBucket: PixelBucket;
    scores: Score[];
}

export interface PixelBucket {
    amount: number;
    maxAmount: number;
    increasePerMinute: number;
}

export interface Score {
    guild: Guild;
    amount: number;
}
