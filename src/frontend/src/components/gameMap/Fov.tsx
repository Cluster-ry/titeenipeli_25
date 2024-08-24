import { mapConfig } from "./MapConfig";
import { useEffect, useState } from "react";


const Fov = () => {
    const fovRange = 10;    // Value for local testing, removed later
    const playerCoordinate = {x: 1, y: 1}

    const [lowerY, setLowerY] = useState(playerCoordinate.y - fovRange);
    const [upperY, setUpperY] = useState(playerCoordinate.y + fovRange);
    const [lowerX, setLowerX] = useState(playerCoordinate.x - fovRange);
    const [upperX, setUpperX] = useState(playerCoordinate.x + fovRange);

    // Current hook for getting the boundaries
    useEffect(() => {
        getBoundaries();
    }, []);

    // For testing whether the boundaries work
    useEffect(() => {
        console.log(upperX)
        console.log(upperY)
        console.log(lowerX)
        console.log(lowerY)
    }, [upperX, upperY, lowerX, lowerY])

    /**
     * Gathering all the pixels that are within the range of the client.
     * 
     * @rules
     */
    const getPixelsInRange = () => {        
    }

    const updatePreliminaryBoundaries = () => {
        setLowerY(playerCoordinate.y - fovRange);
        setUpperY(playerCoordinate.y + fovRange);
        setLowerX(playerCoordinate.x - fovRange);
        setUpperX(playerCoordinate.x + fovRange);    
    }

    /**
     * Getting the fov for-loop caps by seeing if the user is near the map boundaries 
     */
    const getBoundaries = () => {
        
        // Upper Y
        if (upperY > mapConfig.MapHeight) {
            setUpperY(upperY - (playerCoordinate.y + upperY - mapConfig.MapHeight));
        }

        // Lower Y
        if (lowerY < 0) {
            setLowerY(0);
        }

        // Upper X
        if (upperX > mapConfig.MapWidth) {
            setUpperX(upperX - (playerCoordinate.x + upperX - mapConfig.MapWidth));
        }

        // Lower X
        if (lowerX < 0) {
            setLowerX(0);
        }
    }

    return <></>
}

export default Fov; 