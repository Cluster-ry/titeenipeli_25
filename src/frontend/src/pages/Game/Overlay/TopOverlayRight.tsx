import { FC, PropsWithChildren } from "react";
import "./overlay.css";
import HamburgerMenuButton from "../../../components/Ctf/HamburgerMenuButton";

const TopOverlayRight: FC<PropsWithChildren> = () => {
    return (
        <div className="top-overlay__right">
            <HamburgerMenuButton />
        </div>
    );
};

export { TopOverlayRight };
