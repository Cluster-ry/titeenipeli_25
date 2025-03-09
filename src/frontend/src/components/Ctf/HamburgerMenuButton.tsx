import "./hamburger.css";
import { helpModalStore } from "../../stores/helpModalStore";
const HamburgerMenuButton = () => {
    const { setHelpModalOpenState } = helpModalStore();

    return (
        <div className="hamburger-button-wrapper">
            <div className="hamburger" onClick={() => setHelpModalOpenState(true)}>
                <div className="hamburger-bar"></div>
                <div className="hamburger-bar"></div>
                <div className="hamburger-bar"></div>
            </div>
        </div>
    );
};

export default HamburgerMenuButton;
