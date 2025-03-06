import "./graphicsSwitch.css";
import { useSetGraphicsEnabled } from "../../hooks/graphicsHooks/useSetGraphicsEnabled.ts";
import { useGraphicsIndicator } from "../../hooks/graphicsHooks/useDisplayIndicator.ts";

const GraphicsSwitch = () => {
  const graphicsEnabledCallback = useSetGraphicsEnabled();
  const { getIndicatorClasses } = useGraphicsIndicator();
  const { onClass, offClass } = getIndicatorClasses();
  
  return (
    <div className="graphics">
      <span>Switch graphics</span>
      <div
        className="graphics-button"
        onClick={graphicsEnabledCallback}
      >
        <div className={onClass}></div>
        <div className={offClass}></div>
      </div>
    </div>
  );
};

export default GraphicsSwitch;
