import { HslaColour } from "./HslaColour.ts";

/**
 * A rectangle representing a coordinate. A rectangle has the following attributes:
 * 1) Width and Height
 * 2) x-axis and y-axis coordinates
 * 3) A color representing the guild that owns the coordinate
 */
interface ForegroundRectangleProps {
    x: number;
    y: number;
    width: number;
    height: number;
    color: HslaColour;
}

export default ForegroundRectangleProps;
