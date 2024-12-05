import { FC } from "react";
import { useGameState } from "../../../hooks/useGameState";
import { BottomOverlay } from "./BottomOverlay";
import { TopOverlay } from "./TopOverlay";

const GameOverlay: FC = () => {
    useGameState();

    return (
        <div className="overlay">
            <TopOverlay/>
            <BottomOverlay />
        </div>
    );
};

export { GameOverlay };
