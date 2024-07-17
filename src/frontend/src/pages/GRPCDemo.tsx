import { useEffect } from "react";
import { MapUpdateClient } from "../generated/grpc/services/MapUpdateServiceClientPb";
import * as Map from "../generated/grpc/services/MapUpdate_pb";
import * as PixelOwners from "../generated/grpc/components/enums/pixelOwners_pb";
import * as RelativeCoordinate from "../generated/grpc/components/schemas/relativeCoordinate_pb";

export default function GRPCTest() {
  useEffect(() => {
    callGRPC();
  });

  const callGRPC = async () => {
    const mapUpdateClient = new MapUpdateClient(window.location.origin);
    const relativeCoordinate = new RelativeCoordinate.RelativeCoordinate();
    relativeCoordinate.setX(1);
    relativeCoordinate.setY(1);
    const incrementalMapUpdate =
      new Map.IncrementalMapUpdateRequest.IncrementalMapUpdate();
    incrementalMapUpdate.setOwner(PixelOwners.PixelOwners.CLUSTER);
    incrementalMapUpdate.setOwnpixel(true);
    incrementalMapUpdate.setSpawnrelativecoordinate();
    incrementalMapUpdate.setType(1);
    incrementalMapUpdate.setSpawnrelativecoordinate(relativeCoordinate);
    const mapUpdateRequest = new Map.IncrementalMapUpdateRequest();
    mapUpdateRequest.addUpdates(incrementalMapUpdate);
    await mapUpdateClient.getIncremental(mapUpdateRequest);
  };

  return <div id="grpcdemo"></div>;
}
