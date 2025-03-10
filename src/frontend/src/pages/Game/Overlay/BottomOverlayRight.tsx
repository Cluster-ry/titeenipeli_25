import infoIcon from "../../../assets/new_assets/info.png";
import { useHelpModalStore } from "../../../stores/helpModalStore";
import "./overlay.css";

const BottomOverlayRight = () => {
    const setHelpModalOpenState = useHelpModalStore(state => state.setHelpModalOpenState);
    return (
        <div
            className="bottom-overlay__right"
            onClick={() => {
                setHelpModalOpenState(true);
            }}
        >
            <img className="bottom-overlay__right__help" src={infoIcon} />
        </div>
    );
};

export default BottomOverlayRight;
