import "./instructions.css";
import { useInstructionsStore } from "../../stores/instructionsStore";
import Modal from "../Modal/Modal";
import { Instructions } from "../../models/ComponentData/Instructions";
import InstructionsEntry from "./InstructionsEntry";
import { FC, useCallback } from "react";
import mFilesLogo from "../../assets/m_files.svg";
import instructions1 from "../../assets/instructions/instructions_1.webp";
import instructions2 from "../../assets/instructions/instructions_2.webp";

const instructionsData: Instructions[] = [
    {
        header: "May the best guild win!",
        instructions: [
            {text: "Help your guild win by clicking and conquering adjacent pixels", img: instructions1},
            {text: "Conquer swathes of land by encircling areas", img: instructions2},
            {text: "Devastate your rivals by severing their pixels from their spawn points"},
            {text: "Keep an eye on your bucket points! Your guild needs momentum!"},
            {text: "Ticking scores are calculated based on conquered pixels"},
        ],
    },
    {
        header: "Power-ups",
        instructions: [{text: "Manipulate destiny with game-changing special effects!"}, {text: "Receive power-ups via CTF tokens."}],
    },
];

const InstructionsModal: FC = () => {
    const setInstructionsOn = useInstructionsStore(state => state.setInstructionsOn);
    const onClose = useCallback(() => {
        setInstructionsOn(false)
    }, [setInstructionsOn]);
    return (
        <Modal title="Instructions" onClose={onClose}>
            <div className="instruction-container">
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
            </div>
        </Modal>
    );
};

export default InstructionsModal;
