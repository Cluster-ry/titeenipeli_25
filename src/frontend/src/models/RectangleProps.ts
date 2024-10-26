import {HslaColour} from "./HslaColour.ts";

/**
 * A rectangle representing a coordinate. A rectangle has the following attributes:
 * 1) Width and Height 
 * 2) x-axis and y-axis coordinates
 * 3) A color representing the guild that owns the coordinate
 */
interface RectangleProps {
  x: number;
  y: number;
  width: number;
  height: number;
  isSpawn: boolean;
  isOwn: boolean;
  color: HslaColour;
  onClick: (event: { x: number; y: number; }) => void;
}

export default RectangleProps;
