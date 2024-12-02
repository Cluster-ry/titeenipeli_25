import { PropsWithChildren } from "react";
import "./overlay.css";
import Ctf from "../../../components/Ctf";
import { useOverlayStore } from "../../../stores/overlayStore";

type TopOverlayProps = PropsWithChildren<{
    cooldown: number;
    bucketPoints: number;
}>;

const TopOverlay = ({ children: scores, cooldown, bucketPoints }: TopOverlayProps) => {
    const { showHelp, updateShowHelp } = useOverlayStore();

    const alterVisibility = () => {
        updateShowHelp(!showHelp);
    };

    return (
        <div className="top-overlay top-gradient">
            <div className="top-overlay__left">{scores}</div>
            <div className="top-overlay__middle">
                <div className="top-overlay__middle__bucket-points-container">
                    <img className="top-overlay__middle__bucket" src="./src/assets/bucket.png" />
                    <span className="top-overlay__middle__bucket-points">{bucketPoints}</span>
                </div>
                <span className="top-overlay__middle__cooldown">{cooldown}s</span>
            </div>
            <div className="top-overlay__right">
                <img
                    className="top-overlay__right__help"
                    src="./src/assets/help.png"
                    onClick={() => alterVisibility()}
                />
                {showHelp && <Ctf />}
            </div>
        </div>
    );
};

export { TopOverlay };
