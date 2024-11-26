import { FC } from "react";
import { useGameState } from "../../../hooks/useGameState";
import { BottomOverlay } from "./BottomOverlay";
import { Score } from "./Score";
import { TopOverlay } from "./TopOverlay";

const GameOverlay: FC = () => {
    useGameState();

    return (
        <div className="overlay">
            <TopOverlay>
                <Score guild="Cluster" score={16800} />
                <Score guild="Digit" score={14320} />
                <Score guild="TiTe" score={9800} />
            </TopOverlay>
            <BottomOverlay />
        </div>
    );
};

export { GameOverlay };
