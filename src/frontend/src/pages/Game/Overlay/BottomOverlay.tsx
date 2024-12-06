import "./overlay.css";
import special_effect from "../../../assets/special_effect.png";

const BottomOverlay = () => {
    return (
        <div className="bottom-overlay bottom-gradient">
            <div className="bottom-overlay__button">
                <img className="bottom-overlay__button__icon" src={special_effect} />
            </div>
            <span className="bottom-overlay__button__text">Special Effect</span>
        </div>
    );
};

export { BottomOverlay };
