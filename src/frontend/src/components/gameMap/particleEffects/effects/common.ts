import { Coordinate } from "../../../../models/Coordinate";
import { mapConfig } from "../../MapConfig";

export const getPixelCenterCoordinate = ({ x, y }: Coordinate): Coordinate => ({
    x: x * mapConfig.PixelSize + mapConfig.PixelSize / 2,
    y: y * mapConfig.PixelSize + mapConfig.PixelSize / 2,
});
