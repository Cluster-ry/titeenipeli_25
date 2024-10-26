import ViewportBoundigBox from "./ViewportBoundigBox";

interface ViewportProps {
  width: number;
  height: number;
  boundingBox: ViewportBoundigBox;
  children?: React.ReactNode;
}

export default ViewportProps;
