import { MutableRefObject } from "react";
import { Coordinate } from "./Coordinate";

interface MapTileProps {
    x: number;
    y: number;
    width: number;
    height: number;
    backgroundGraphic?: Uint8Array;
    hue?: number,
    alpha?: number,
    saturation?: number,
    lightness?: number
    highlight: boolean;
    moving: MutableRefObject<boolean>;
    onClickRef: MutableRefObject<((coordinate: Coordinate, highlight: boolean) => void) | null>;
}

export default MapTileProps;
