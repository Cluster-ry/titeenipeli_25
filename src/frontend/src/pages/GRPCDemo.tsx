import { useEffect } from "react";
import * as Map from "../generated/grpc/services/MapUpdate_pb.d";
import * as PixelOwners from "../generated/grpc/components/enums/pixelOwners_pb.d";
import * as RelativeCoordinate from "../generated/grpc/components/schemas/relativeCoordinate_pb.d";
import {MapUpdateClient} from "../generated/grpc/services/MapUpdate.client";
import {GrpcWebFetchTransport} from "@protobuf-ts/grpcweb-transport";

export default function GRPCTest() {
  useEffect(() => {
    callGRPC();
  });

  const callGRPC = async () => {
    const transport = new GrpcWebFetchTransport({
      baseUrl: window.location.origin
    });

    const mapUpdateClient = new MapUpdateClient(transport);
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
