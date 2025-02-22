import "./instructions.css"
import { instructionsStore } from "../../stores/instructionsStore"
import Modal from "../Modal/Modal";
import Instructions from "../../models/ComponentData/Instructions";
import InstructionsEntry from "./InstructionsEntry";

const instructionsData: Instructions[] = [
  {
    header: "Contributing",
    instructions: [
      `
        Help your guild win by conquering pixels. This is done by clicking unoccupied
        pixels.
      `,

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
