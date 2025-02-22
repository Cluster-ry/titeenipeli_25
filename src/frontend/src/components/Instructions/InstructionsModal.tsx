import "./instructions.css"
import { instructionsStore } from "../../stores/instructionsStore"
import Modal from "../Modal/Modal";

const instructionsData = [

];

const InstructionsModal = () => {
  const { setInstructionsOn } = instructionsStore();
  
  const fillerText: string = `
    Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.
  `
 
  return (
    <Modal title="Instructions" onClose={() => setInstructionsOn(false)}>
      <div id="instructions">
        <section className="section-text">
          <p>{fillerText}</p>
        </section>
      </div>
    </Modal>
  );
} 

export default InstructionsModal;
