import ViewportBoundingBox from "./ViewportBoundingBox.ts";

interface ViewportProps {
    width: number;
    height: number;
    boundingBox: ViewportBoundingBox;
    children?: React.ReactNode;
    onMoveStart: () => void;
}

export default ViewportProps;
