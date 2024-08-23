import { mapConfig } from "./MapConfig";
import { useEffect, useState } from "react";


const Fov = () => {
    const fovRange = 10;    // Value for local testing, removed later

    const [lowerY, setLowerY] = useState(fovRange);
    const [upperY, setUpperY] = useState(fovRange);
    const [lowerX, setLowerX] = useState(fovRange);
    const [upperX, setUpperX] = useState(fovRange);

    // Current hook for getting the boundaries
    useEffect(() => {
        getBoundaries();
    }, []);

    const playerCoordinate = {x: 1, y: 1}
    /**
     * Gathering all the pixels that are within the range of the client.
     * 
     * @rules
     */
    const getPixelsInRange = () => {        
    }


    /**
     * Getting the fov for-loop caps by seeing if the user is near the map boundaries 
     */
    const getBoundaries = () => {
        
        // Upper Y
        if (playerCoordinate.y + fovRange > mapConfig.MapHeight) {
            setUpperY(upperY - (playerCoordinate.y + upperY - mapConfig.MapHeight));
        }

        // Lower Y
        if (playerCoordinate.y - fovRange < 0) {
            setLowerY(lowerY - (-(playerCoordinate.y - fovRange)));
        }

        // Upper X
        if (playerCoordinate.x + fovRange > mapConfig.MapWidth) {
            setUpperX(upperX - (playerCoordinate.x + upperX - mapConfig.MapWidth));
        }

        // Lower X
        if (playerCoordinate.x - fovRange < 0) {
            setLowerX(lowerX - (-(playerCoordinate.x - fovRange)));
        }
    }
}