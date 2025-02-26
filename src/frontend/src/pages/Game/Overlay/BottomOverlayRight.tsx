import infoIcon from "../../../assets/new_assets/info.png"
import "./overlay.css"

const BottomOverlayRight = () => {
  return (
    <div className="bottom-overlay__right">
      <img className="bottom-overlay__right__help" src={infoIcon} />
    </div>
  );
}

export default BottomOverlayRight;
