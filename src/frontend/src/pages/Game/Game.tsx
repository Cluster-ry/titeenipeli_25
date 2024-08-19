import { ReactNode } from "react";
import { GameOverlay } from "./Overlay/GameOverlay";
import "./game.css";

type GameProps = {
  slot: ReactNode;
};

function Game({ slot }: GameProps) {
  return (
    <div className="game-container">
      <div className="slot-container">{slot}</div>
      <GameOverlay />
    </div>
  );
}

export { Game };
