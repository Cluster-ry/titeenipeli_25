import { FC, PropsWithChildren } from "react";
import { Scores } from "./Scores";
import "./overlay.css";
import { TopOverlayRight } from "./TopOverlayRight";

const TopOverlay: FC<PropsWithChildren> = () => {
    return (
        <div className="top-overlay top-gradient">
            <Scores />
            <TopOverlayRight />
        </div>
    );
};

export { TopOverlay };
