import { FC, PropsWithChildren } from "react";
import { RatelimitMeter } from "./RatelimitMeter";
import { Scores } from "./Scores";
import help from "../../../assets/help.png";
import "./overlay.css";
import { useOverlayStore } from "../../../stores/overlayStore";
import HamburgerMenu from "../../../components/Ctf/HamburgerMenu";

const TopOverlay: FC<PropsWithChildren> = () => {
    const { showHelp, updateShowHelp } = useOverlayStore();
    const alterVisibility = () => {
        updateShowHelp(!showHelp);
    };

    return (
        <div className="top-overlay top-gradient">
            <Scores />
            <RatelimitMeter />
            <div className="top-overlay__right">
              <img className="top-overlay__right__help" src={help} onClick={() => alterVisibility()}/>        
                {showHelp && <HamburgerMenu />}
            </div>
        </div>
    );
};

export { TopOverlay };
