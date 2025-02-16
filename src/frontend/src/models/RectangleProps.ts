import { MutableRefObject } from "react";
import { HslaColour } from "./HslaColour";

interface RectangleProps {
    x: number;
    y: number;
    width: number;
    height: number;
    backgroundGraphic?: Uint8Array;
    color?: HslaColour;
    moving: MutableRefObject<boolean>;
    highlight: boolean;
    onClick: (event: { x: number; y: number }) => void;
}

export default RectangleProps;
