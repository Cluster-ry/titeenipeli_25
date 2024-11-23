import { useGameState } from "../../../hooks/useGameState";
import { BottomOverlay } from "./BottomOverlay";
import { Score } from "./Score";
import { TopOverlay } from "./TopOverlay";

const GameOverlay = () => {
    useGameState();

    return (
        <div className="overlay">
            <TopOverlay>
                <Score guild="Cluster" score={16800} position={1} />
                <Score guild="Digit" score={14320} position={2} />
                <Score guild="TiTe" score={9800} position={3} />
            </TopOverlay>
            <BottomOverlay />
        </div>
    );
};

export { GameOverlay };
