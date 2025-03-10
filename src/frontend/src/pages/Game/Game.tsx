import { ReactNode } from "react";
import { GameOverlay } from "./Overlay/GameOverlay";
import "./game.css";
import { helpModalStore } from "../../stores/helpModalStore";
import HelpModal from "../../components/Ctf/HelpModal";
import Notification from "../../components/Notification";
import { instructionsStore } from "../../stores/instructionsStore";
import InstructionsModal from "../../components/Instructions/InstructionsModal";

type GameProps = {
    slot: ReactNode;
};

const Game = ({ slot }: GameProps) => {
    const { helpModalOpen } = helpModalStore();
    const { instructionsOn } = instructionsStore();
    return (
        <div className="game-container">
            <div className="slot-container">{slot}</div>
            <GameOverlay />
            {helpModalOpen && <HelpModal />}
            {instructionsOn && <InstructionsModal />}
            <Notification />
        </div>
    );
};

export { Game };
