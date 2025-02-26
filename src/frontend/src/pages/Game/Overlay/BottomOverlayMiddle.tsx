import "./overlay.css"
import powerupIcon from "../../../assets/sprites/powerup.png"

const BottomOverlayMiddle = () => {
  return (
    <div className="bottom-overlay__middle">
      <img className="bottom-overlay__middle__powerup" src={powerupIcon} />
    </div>
  );
}

export default BottomOverlayMiddle;
