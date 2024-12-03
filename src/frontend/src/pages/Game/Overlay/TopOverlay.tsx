import { FC, PropsWithChildren } from "react";
import { RatelimitMeter } from "./RatelimitMeter";
import { Scores } from "./Scores";
import help from "../../../assets/help.png";
import "./overlay.css";

const TopOverlay: FC<PropsWithChildren> = () => {
    return (
        <div className="top-overlay top-gradient">
            <Scores />
            <RatelimitMeter />
            <div className="top-overlay__right">
                <img className="top-overlay__right__help" src={help} />
            </div>
        </div>
    );
};

export { TopOverlay };
