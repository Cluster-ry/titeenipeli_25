import { HslaColour } from "../models/HslaColour";
import { Pixel } from "../models/Pixel";
import { User } from "../models/User";

/**
 * A pixel owned by the current user will have its saturation lowered
 */ 
const checkUserOwnedPixels = (user: User | null, pixel: Pixel | null, color: HslaColour): HslaColour => {
  
  if (user === null || pixel === null) {
    return color;
  }

  if (pixel.owner !== user.id || pixel.guild !== user.guild) {
    return color;
  }
 
  const alteredColor: HslaColour = {
    hue: color.hue, 
    saturation: color.saturation - 20,
    lightness: color.lightness
  }  

  return alteredColor;
}

export default checkUserOwnedPixels;
