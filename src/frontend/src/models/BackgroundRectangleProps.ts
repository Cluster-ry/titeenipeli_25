interface BackgroundRectangleProps {
    x: number;
    y: number;
    width: number;
    height: number;
    backgroundGraphic: Uint8Array;
    highlight: boolean;
    onClick: (event: { x: number; y: number }) => void;
}

export default BackgroundRectangleProps;
