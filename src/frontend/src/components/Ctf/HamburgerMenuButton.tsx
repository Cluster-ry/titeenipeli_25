import "./hamburger.css";
import { useHelpModalStore } from "../../stores/helpModalStore";
import { useCallback } from "react";
const HamburgerMenuButton = () => {
    const setHelpModalOpenState = useHelpModalStore(state => state.setHelpModalOpenState);
    const onClick = useCallback(() => {
        setHelpModalOpenState(true);
    }, [setHelpModalOpenState]);
    return (
        <div className="hamburger-button-wrapper">
            <div className="hamburger" onClick={onClick}>
                <div className="hamburger-bar"></div>
                <div className="hamburger-bar"></div>
                <div className="hamburger-bar"></div>
            </div>
        </div>
    );
};

export default HamburgerMenuButton;
