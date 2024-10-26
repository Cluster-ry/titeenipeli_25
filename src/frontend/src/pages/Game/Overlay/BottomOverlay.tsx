import "./overlay.css";

const BottomOverlay = () => {
    return (
        <div className="bottom-overlay bottom-gradient">
            <div className="bottom-overlay__button">
                <img className="bottom-overlay__button__icon" src="./src/assets/special_effect.png" />
            </div>
            <span className="bottom-overlay__button__text">Special Effect</span>
        </div>
    );
};

export { BottomOverlay };
