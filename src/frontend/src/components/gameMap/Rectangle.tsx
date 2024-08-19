import { Sprite } from "@pixi/react";
import { Texture } from "pixi.js";
//import { PixiComponent } from "@pixi/react";
//import { FederatedPointerEvent, Graphics, Texture } from "pixi.js";

interface RectangleProps {
  x: number;
  y: number;
  width: number;
  height: number;
  color: number;
  onClick: (event: { x: number; y: number; color: number }) => void;
}

/*const Rectangle = PixiComponent<RectangleProps, Graphics>("Rectangle", {
  create: () => new Graphics(),
  applyProps: (ins, _, props) => {
    ins.x = props.x;
    ins.y = props.y;
    ins.beginFill(props.color);
    ins.drawRect(0, 0, props.width, props.height);
    ins.endFill();
    ins.cullable = true;
    ins.eventMode = "static";
    ins.on("pointertap", props.onClick);
  },
});*/

const Rectangle = (props: RectangleProps) => {
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
