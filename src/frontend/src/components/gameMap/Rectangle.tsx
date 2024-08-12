import { PixiComponent } from "@pixi/react";
import { Graphics } from "pixi.js";

interface RectangleProps {
  x: number;
  y: number;
  width: number;
  height: number;
  color: number;
}

const Rectangle = PixiComponent<RectangleProps, Graphics>("Rectangle", {
  create: () => new Graphics(),
  applyProps: (ins, _, props) => {
    ins.x = props.x
    ins.y = props.y
    ins.beginFill(props.color);
    ins.drawRect(0, 0, props.width, props.height);
    ins.endFill();
    ins.cullable = true;
  },
});

export default Rectangle;
