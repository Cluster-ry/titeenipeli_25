import "./hamburger.css";
import Ctf from "./Ctf";
import { useState } from "react";
const HamburgerMenu = () => {
    const [showMenu, setShowMenu] = useState(false);

    return (
        <div className="hamburger-wrapper">
            <div className="hamburger" onClick={() => setShowMenu(!showMenu)}>
                <div className="hamburger-bar"></div>
                <div className="hamburger-bar"></div>
                <div className="hamburger-bar"></div>
            </div>
            {showMenu && <Ctf />}
        </div>
    );
};

export default HamburgerMenu;
