import "./instructions.css";
import Instructions from "../../models/ComponentData/Instructions";

const InstructionsEntry = (instructions: Instructions) => {
    return (
        <ul>
            <h2>{instructions.header}</h2>
            {instructions.instructions.map((entry: string, index: number) => (
                <li key={index} className="entry-text">{entry}</li>
            ))}
        </ul>
    );
};

export default InstructionsEntry;
