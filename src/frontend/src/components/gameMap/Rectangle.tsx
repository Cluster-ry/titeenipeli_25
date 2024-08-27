import { Sprite } from "@pixi/react";
import { Texture } from "pixi.js";

interface RectangleProps {
  x: number;
  y: number;
  width: number;
  height: number;
  color: number;
  onClick: (event: { x: number; y: number; color: number }) => void;
}

const Rectangle = (props: RectangleProps) => {

  // When a client clicks a pixel
  const handleClick = () => {
    props.onClick({ x: props.x, y: props.y, color: props.color });
  };

  return (
    <Sprite
      position={{ x: props.x, y: props.y }}
      tint={props.color}
      texture={Texture.WHITE}
      width={props.width}
      height={props.height}
      cullable={true}
      eventMode="dynamic"
      pointertap={handleClick}
    />
  );
};

export default Rectangle;
