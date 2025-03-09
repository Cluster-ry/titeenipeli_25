import "./instructions.css";
import { instructionsStore } from "../../stores/instructionsStore";
import Modal from "../Modal/Modal";
import Instructions from "../../models/ComponentData/Instructions";
import InstructionsEntry from "./InstructionsEntry";
import { FC } from "react";
import background_logo from "../../assets/M-Files-Logo-With-Tagline-Full-Color-RGB.svg";

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
    const { setInstructionsOn } = instructionsStore();
    return (
        <Modal title="Instructions" onClose={() => setInstructionsOn(false)}>
            <div id="instructions">
                <div className="section-text">
                    {instructionsData.map((instructionsEntry: Instructions) => (
                        <InstructionsEntry key={instructionsEntry.header} {...instructionsEntry} />
                    ))}
                </div>
            </div>
            <h2>Sponsored by:</h2>
            <img style={{
                width: '50%',
            }} src={background_logo}/>
        </Modal>
    );
};

export default InstructionsModal;
