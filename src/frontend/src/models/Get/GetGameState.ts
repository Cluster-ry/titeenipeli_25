import Guild from "../enum/Guild";

export interface GetGameState {
    pixelBucket: PixelBucket;
    scores: Score[];
}

interface PixelBucket {
    amount: number;
    maxAmount: number;
    increasePerMinute: number;
}

interface Score {
    guild: Guild;
    amount: number;
}
