export interface Instruction {
    text: string;
    img?: string;
};
export interface Instructions {
    header: string;
    instructions: Instruction[];
}
