import { useCallback } from "react";
import { useGraphicsStore } from "../../stores/graphicsStore";

export const useGraphicsIndicator = () => {
  const graphicsEnabled = useGraphicsStore(state => state.graphicsEnabled);
  const setGraphicsEnabled = useGraphicsStore(state => state.setGraphicsEnabled);

  const toggleGraphics = useCallback(() => {
    setGraphicsEnabled(!graphicsEnabled);
  }, [graphicsEnabled, setGraphicsEnabled]);

  const getIndicatorClasses = () => {
    return graphicsEnabled
      ? {
          onClass: "indicator-on",
          offClass: "indicator-off hide",
        }
      : {
          onClass: "indicator-on hide",
          offClass: "indicator-off",
        };
  };

  return {
    toggleGraphics,
    getIndicatorClasses,
  };
};

