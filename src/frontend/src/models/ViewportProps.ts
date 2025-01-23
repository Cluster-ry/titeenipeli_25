import ViewportBoundingBox from "./ViewportBoundingBox.ts";

interface ViewportProps {
    width: number;
    height: number;
    boundingBox: ViewportBoundingBox;
    children?: React.ReactNode;
    onMoveStart: () => void;
    onMoveEnd: () => void;
}

export default ViewportProps;
