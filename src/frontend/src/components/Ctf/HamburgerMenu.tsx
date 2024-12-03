import "./hamburger.css"
import CtfWrapper from "./CtfWrapper";
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
      {showMenu && <CtfWrapper />}
    </div>
  );
};

export default HamburgerMenu;

