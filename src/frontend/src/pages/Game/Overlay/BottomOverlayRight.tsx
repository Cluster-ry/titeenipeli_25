import infoIcon from "../../../assets/new_assets/info.png";
import { useHelpModalStore } from "../../../stores/helpModalStore";
import "./overlay.css";
import { useCallback } from "react";

const BottomOverlayRight = () => {
    const setHelpModalOpenState = useHelpModalStore(state => state.setHelpModalOpenState);
    const openHelpModal = useCallback(() => {
        setHelpModalOpenState(true);
    }, [setHelpModalOpenState]);
    return (
        <div className="bottom-overlay__right" onClick={openHelpModal}>
            <img className="bottom-overlay__right__help low-res" src={infoIcon} />
        </div>
    );
};

export default BottomOverlayRight;
