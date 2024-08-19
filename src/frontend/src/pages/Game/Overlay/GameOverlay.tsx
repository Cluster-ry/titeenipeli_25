import { BottomOverlay } from "./BottomOverlay";
import { Score } from "./Score";
import { TopOverlay } from "./TopOverlay";

function GameOverlay() {
  return (
    <div className="overlay">
      <TopOverlay cooldown={0} bucketPoints={100}>
        <Score guild="Cluster" score={16800} position={1} />
        <Score guild="Digit" score={14320} position={2} />
        <Score guild="TiTe" score={9800} position={3} />
      </TopOverlay>
      <BottomOverlay />
    </div>
  );
}

export { GameOverlay };
