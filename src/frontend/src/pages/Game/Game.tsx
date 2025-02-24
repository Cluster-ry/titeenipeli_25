import { ReactNode } from "react";
import { GameOverlay } from "./Overlay/GameOverlay";
import "./game.css";
import { useCtfStore } from "../../stores/ctfModalStore";
import CtfModal from "../../components/Ctf/CtfModal";
import Notification from "../../components/Notification";
import { instructionsStore } from "../../stores/instructionsStore";
import InstructionsModal from "../../components/Instructions/InstructionsModal";

type GameProps = {
    slot: ReactNode;
};

const Game = ({ slot }: GameProps) => {
    const { ctfModelOpen: hamburgerMenuOpen } = useCtfStore();
    const { instructionsOn } = instructionsStore();
    return (
        <div className="game-container">
            <div className="slot-container">{slot}</div>
            <GameOverlay />
            {hamburgerMenuOpen && <CtfModal />}
            {instructionsOn && <InstructionsModal />}
            <Notification />
        </div>
    );
};

export { Game };
