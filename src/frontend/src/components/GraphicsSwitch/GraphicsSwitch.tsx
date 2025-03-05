import "./graphicsSwitch.css"
import { graphicsStore } from "../../stores/graphicsStore.ts";
import { useNotificationStore } from "../../stores/notificationStore.ts";

const GraphicsSwitch = () => {
  const { graphicsEnabled, setGraphicsEnabled } = graphicsStore();
    const { triggerNotification } = useNotificationStore();
  
  return (
    <div className="graphics">
      <span>Switch graphics</span>
      <div className="graphics-button"
        onClick={() => {
        setGraphicsEnabled(!graphicsEnabled)
        !graphicsEnabled
          ? triggerNotification("Graphics enabled", "success")
          : triggerNotification("Graphics disabled", "success")

        }}
      >
        {
        graphicsEnabled
          ? 
            <>
              <div className="indicator-on"></div>
              <div className="indicator-off indicator-disabled"></div>
            </>
          : <>
              <div className="indicator-on indicator-disabled"></div>
              <div className="indicator-off"></div>
            </>
        }
      </div>
    </div>
  );
}

export default GraphicsSwitch;
