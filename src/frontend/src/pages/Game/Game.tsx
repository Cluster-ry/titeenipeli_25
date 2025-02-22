import { ReactNode } from "react";
import { GameOverlay } from "./Overlay/GameOverlay";
import "./game.css";
import { useCtfStore } from "../../stores/ctfModalStore";
import CtfModal from "../../components/Ctf/CtfModal";
import Notification from "../../components/Notification";

type GameProps = {
    slot: ReactNode;
};

const Game = ({ slot }: GameProps) => {
    const { ctfModelOpen: hamburgerMenuOpen } = useCtfStore();

    return (
        <div className="game-container">
            <div className="slot-container">{slot}</div>
            <GameOverlay />
            {hamburgerMenuOpen && <CtfModal />}
            <Notification />
        </div>
    );
};

export { Game };
