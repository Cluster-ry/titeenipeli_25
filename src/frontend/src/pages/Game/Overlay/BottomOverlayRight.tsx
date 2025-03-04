import infoIcon from "../../../assets/new_assets/info.png";
import "./overlay.css";
import { useCtfStore } from "../../../stores/ctfModalStore";

const BottomOverlayRight = () => {
    const { setCtfModelOpenState } = useCtfStore();
    return (
        <div
            className="bottom-overlay__right"
            onClick={() => {
                setCtfModelOpenState(true);
            }}
        >
            <img className="bottom-overlay__right__help" src={infoIcon} />
        </div>
    );
};

export default BottomOverlayRight;
