import "./overlay.css";
import { RatelimitMeter } from "./RatelimitMeter";
import { Scores } from "./Scores";

const TopOverlay = () => {
    return (
        <div className="top-overlay top-gradient">
            <Scores />
            <RatelimitMeter />
            <div className="top-overlay__right">
                <img className="top-overlay__right__help" src="./src/assets/help.png" />
            </div>
        </div>
    );
};

export { TopOverlay };
