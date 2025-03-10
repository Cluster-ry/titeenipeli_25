import { FC, PropsWithChildren } from "react";
import { Scores } from "./score/Scores";
import "./overlay.css";
import { RatelimitMeter } from "./RatelimitMeter";

const TopOverlay: FC<PropsWithChildren> = () => {
    return (
        <div className="top-overlay">
            <Scores />
            <RatelimitMeter />
        </div>
    );
};

export { TopOverlay };
