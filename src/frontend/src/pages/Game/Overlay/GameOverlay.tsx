import { FC } from "react";
import { useGameState } from "../../../hooks/useGameState";
import { BottomOverlay } from "./BottomOverlay";
import { TopOverlay } from "./TopOverlay";
import Notification from "../../../components/Notification";
const GameOverlay: FC = () => {
    useGameState();

    return (
        <div className="overlay">
            <TopOverlay />
            <BottomOverlay />
            <Notification />
        </div>
    );
};

export { GameOverlay };
