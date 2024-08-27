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
        updatePlayerBoundaries();
    }, []);

    // For testing whether the boundaries work
    useEffect(() => {
        console.log(upperX)
        console.log(upperY)
        console.log(lowerX)
        console.log(lowerY)
    }, [upperX, upperY, lowerX, lowerY])


    const updatePlayerBoundaries = () => {
        setLowerY(playerCoordinate.y - fovRange);
        setUpperY(playerCoordinate.y + fovRange);
        setLowerX(playerCoordinate.x - fovRange);
        setUpperX(playerCoordinate.x + fovRange);    
    }

    return <></>
}

export default Fov; 