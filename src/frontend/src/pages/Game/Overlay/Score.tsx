import { FC, useEffect, useState } from "react";
import "./overlay.css";

import goldMedal from "../../../assets/sprites/medal-gold.png"
import silverMedal from "../../../assets/sprites/medal-silver.png"
import bronzeMedal from "../../../assets/sprites/medal-bronze.png"



type ScoreProps = {
    guild: string;
    score: number;
    place: number;
};


const Score: FC<ScoreProps> = ({ guild, score, place }) => {
  const [displayMedal, setDisplayMedal] = useState<string>("");

  useEffect(() => {
    switch(place) {
      case 0:
        setDisplayMedal(goldMedal);
        break;
      case 1:
        setDisplayMedal(silverMedal);
        break;
      case 2:
        setDisplayMedal(bronzeMedal);
        break;
      default:
        break;
    }
  }, []);

  return (
        <div key={guild} className="top-overlay__score-container">
            {
              displayMedal.length > 0 
                ? <img src={displayMedal} />
                : <></>
            }
            <span>{guild}:</span>
            <span>{score}</span>
        </div>
    );
};

export { Score };
