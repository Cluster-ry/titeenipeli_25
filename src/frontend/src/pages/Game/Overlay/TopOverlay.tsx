import { PropsWithChildren } from "react";
import "./overlay.css";
import bucket from "../../../assets/bucket.png";
import help from "../../../assets/help.png";

type TopOverlayProps = PropsWithChildren<{
    cooldown: number;
    bucketPoints: number;
}>;

const TopOverlay = ({ children: scores, cooldown, bucketPoints }: TopOverlayProps) => {
    return (
        <div className="top-overlay top-gradient">
            <div className="top-overlay__left">{scores}</div>
            <div className="top-overlay__middle">
                <div className="top-overlay__middle__bucket-points-container">
                    <img className="top-overlay__middle__bucket" src={bucket} />
                    <span className="top-overlay__middle__bucket-points">{bucketPoints}</span>
                </div>
                <span className="top-overlay__middle__cooldown">{cooldown}s</span>
            </div>
            <div className="top-overlay__right">
                <img className="top-overlay__right__help" src={help} />
            </div>
        </div>
    );
};

export { TopOverlay };
