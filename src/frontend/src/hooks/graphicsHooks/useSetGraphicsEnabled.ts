import { useCallback } from "react"
import { useGraphicsStore } from "../../stores/graphicsStore";
import { useNotificationStore } from "../../stores/notificationStore";

export const useSetGraphicsEnabled = () => {
  const graphicsEnabled = useGraphicsStore(state => state.graphicsEnabled);
  const setGraphicsEnabled = useGraphicsStore(state => state.setGraphicsEnabled);
  const triggerNotification = useNotificationStore(state => state.triggerNotification);
  
  const graphicsEnabledCallback =  useCallback(() => {
    const oldValue = graphicsEnabled;
    setGraphicsEnabled(!oldValue);
    triggerNotification(oldValue ? "Graphics disabled" : "Graphics enabled", "success");
  }, [graphicsEnabled, setGraphicsEnabled, triggerNotification]);

  return graphicsEnabledCallback;
}
