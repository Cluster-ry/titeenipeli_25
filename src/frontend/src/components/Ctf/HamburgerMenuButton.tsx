import "./hamburger.css";
import { useCtfStore } from "../../stores/ctfModalStore";
const HamburgerMenuButton = () => {
    const { setCtfModelOpenState } = useCtfStore();

    return (
        <div className="hamburger-button-wrapper">
            <div className="hamburger" onClick={() => setCtfModelOpenState(true)}>
                <div className="hamburger-bar"></div>
                <div className="hamburger-bar"></div>
                <div className="hamburger-bar"></div>
            </div>
        </div>
    );
};

export default HamburgerMenuButton;
