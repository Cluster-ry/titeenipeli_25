import { useGameStateStore } from "../../../stores/gameStateStore";
import bucketImage from "../../../assets/bucket.png";
import "./overlay.css";

const RatelimitMeter = () => {
    const bucket = useGameStateStore((state) => state.pixelBucket);

    return (
        <div className="top-overlay__middle">
            <div className="top-overlay__middle__bucket-points-container">
                <img className="top-overlay__middle__bucket" src={bucketImage} />
                <span className="top-overlay__middle__bucket-points">
                    {bucket.amount}/{bucket.maxAmount}
                </span>
            </div>
            <span className="top-overlay__middle__cooldown">{Math.round(bucket.increasePerMinute * 10) / 10}/min</span>
        </div>
    );
};

export { RatelimitMeter };
