import Guild from "../enum/Guild";

export interface GetGameState {
    pixelBucket: PixelBucket;
    scores: Score[];
    powerUps: PowerUp[];
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

export interface PowerUp {
    powerUpId: number;
    name: string;
    description: string;
    directed: boolean;
}
