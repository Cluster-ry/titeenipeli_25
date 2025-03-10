import "./instructions.css";
import { useInstructionsStore } from "../../stores/instructionsStore";
import Modal from "../Modal/Modal";
import Instructions from "../../models/ComponentData/Instructions";
import InstructionsEntry from "./InstructionsEntry";
import { FC, useCallback } from "react";
import mFilesLogo from "../../assets/M-Files-Logo-With-Tagline-Full-Color-RGB.svg";

const instructionsData: Instructions[] = [
    {
        header: "May the best guild win!",
        instructions: [
            "Help your guild win by clicking and conquering pixels!",
            "Ticking scores are calculated based on conquered pixels.",
            "Devastate your rivals by severing their pixels from their spawn points!",
            "Keep an eye on your bucket points! Your guild needs momentum!",
        ],
    },
    {
        header: "Power-ups",
        instructions: ["Manipulate destiny with game-changing special effects!", "Receive power-ups via CTF tokens."],
    },
];

const InstructionsModal: FC = () => {
    const setInstructionsOn = useInstructionsStore(state => state.setInstructionsOn);
    const onClose = useCallback(() => {
        setInstructionsOn(false)
    }, [setInstructionsOn]);
    return (
        <Modal title="Instructions" onClose={onClose}>
            <div id="instructions">
                <div className="section-text">
                    {instructionsData.map((instructionsEntry: Instructions) => (
                        <InstructionsEntry key={instructionsEntry.header} {...instructionsEntry} />
                    ))}
                </div>
            </div>
            <div className="sponsor-container">
                <h3>Sponsored by:</h3>
                <img className="icon" src={mFilesLogo}/>
            </div>
        </Modal>
    );
};

export default InstructionsModal;
