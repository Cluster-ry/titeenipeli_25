import Instructions from "../../models/ComponentData/Instructions";

const InstructionsEntry = (instructions: Instructions) => {
  return (
    <div>
      <h2>{instructions.header}</h2>
      {
        instructions.instructions.map((entry: string) => (
          <p>{entry}</p>
        ))
      }
    </div>
  );
}

export default InstructionsEntry;
