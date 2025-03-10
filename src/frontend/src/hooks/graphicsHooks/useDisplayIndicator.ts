import { useCallback } from "react";
import { graphicsStore } from "../../stores/graphicsStore";

export const useGraphicsIndicator = () => {
  const { graphicsEnabled, setGraphicsEnabled } = graphicsStore();

  const toggleGraphics = useCallback(() => {
    setGraphicsEnabled(!graphicsEnabled);
  }, [graphicsEnabled, setGraphicsEnabled]);

  const getIndicatorClasses = () => {
    return graphicsEnabled
      ? {
          onClass: "indicator-on",
          offClass: "indicator-off indicator-disabled",
        }
      : {
          onClass: "indicator-on indicator-disabled",
          offClass: "indicator-off",
        };
  };

  return {
    toggleGraphics,
    getIndicatorClasses,
  };
};

