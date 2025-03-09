import { FC, PropsWithChildren } from "react";
import { useGameStateStore } from "../../../stores/gameStateStore";
import "./overlay.css";
import bucketImage from "../../../assets/bucket.png";

const RatelimitMeter: FC<PropsWithChildren> = () => {
    const bucket = useGameStateStore((state) => state.pixelBucket);
    return (
        <div className="top-overlay__right">
            <img className="top-overlay__right__bucket" src={bucketImage} alt="Pixel Bucket" />
            <div className="top-overlay__right__spans">
                <span className="top-overlay__right__cooldown">
                    {Math.round(bucket.increasePerMinute * 10) / 10} px / min
                </span>
                <span className="top-overlay__right__bucket-points">
                    {bucket.amount} / {bucket.maxAmount} px
                </span>
            </div>
        </div>
    );
};

export { RatelimitMeter };
