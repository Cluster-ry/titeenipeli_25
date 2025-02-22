import "./instructions.css"
import { instructionsStore } from "../../stores/instructionsStore"
import Modal from "../Modal/Modal";
import Instructions from "../../models/ComponentData/Instructions";
import InstructionsEntry from "./InstructionsEntry";

const instructionsData: Instructions[] = [
  {
    header: "May the best guild win!",
    instructions: [
      "Help your guild win by clicking and conquering pixels!",
      "Devastate your rivals by severing their pixels from their spawn points!",
      "Keep an eye on your bucket points! Your guild needs momentum!",
      "Manipulate destiny with game-changing special effects!"
    ]
  }
];

const InstructionsModal = () => {
  const { setInstructionsOn } = instructionsStore();
  
  return (
    <Modal title="Instructions" onClose={() => setInstructionsOn(false)}>
      <div id="instructions">
        <div className="section-text">
          {
            instructionsData.map((instructionsEntry: Instructions) => (
              <InstructionsEntry {...instructionsEntry} />
            ))
          }
        </div>
      </div>
    </Modal>
  );
} 

export default InstructionsModal;
