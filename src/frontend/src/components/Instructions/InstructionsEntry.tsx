import "./instructions.css";
import { Instructions, Instruction } from "../../models/ComponentData/Instructions";
import { useMemo } from "react";

const InstructionsEntry = ({ header, instructions }: Instructions) => {
    const entries = useMemo(() =>
        instructions.map((entry: Instruction, index: number) => (
            <li key={index} className="entry-text" >
                {entry.text}
                {entry.img && <><br/> <img src={entry.img} loading="lazy" className="entry-image" /></>}
                </li>
        )), [instructions]);
    return (
        <ul>
            <h2>{header}</h2>
            {entries}
        </ul>
    );
};

export default InstructionsEntry;
