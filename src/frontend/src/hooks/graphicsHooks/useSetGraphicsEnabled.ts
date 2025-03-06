import { useCallback } from "react"
import { graphicsStore } from "../../stores/graphicsStore";
import { useNotificationStore } from "../../stores/notificationStore";

export const useSetGraphicsEnabled = () => {
  const { graphicsEnabled, setGraphicsEnabled } = graphicsStore();
  const { triggerNotification } = useNotificationStore();
  
  const graphicsEnabledCallback =  useCallback(() => {
    setGraphicsEnabled(!graphicsEnabled);
    !graphicsEnabled
      ? triggerNotification("Graphics enabled", "success")
      : triggerNotification("Graphics disabled", "success");
  }, [graphicsEnabled]);

  return graphicsEnabledCallback;
}
