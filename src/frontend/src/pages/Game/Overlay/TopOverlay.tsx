import { FC, PropsWithChildren } from "react";
import { RatelimitMeter } from "./RatelimitMeter";
import { Scores } from "./Scores";
import "./overlay.css";
import HamburgerMenu from "../../../components/Ctf/HamburgerMenu";

const TopOverlay: FC<PropsWithChildren> = () => {
    return (
        <div className="top-overlay top-gradient">
            <Scores />
            <RatelimitMeter />
            <div className="top-overlay__right">
                <HamburgerMenu />
            </div>
        </div>
    );
};

export { TopOverlay };
