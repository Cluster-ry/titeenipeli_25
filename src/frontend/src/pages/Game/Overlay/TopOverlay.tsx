import { FC, PropsWithChildren } from "react";
import { RatelimitMeter } from "./RatelimitMeter";
import { Scores } from "./Scores";
import "./overlay.css";
import { TopOverlayRight } from "./TopOverlayRight";

const TopOverlay: FC<PropsWithChildren> = () => {
    return (
        <div className="top-overlay top-gradient">
            <Scores />
            <RatelimitMeter />
            <TopOverlayRight />
        </div>
    );
};

export { TopOverlay };
