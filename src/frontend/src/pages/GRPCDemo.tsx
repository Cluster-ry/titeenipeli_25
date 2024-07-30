import { useEffect } from "react";
import {GrpcWebFetchTransport} from "@protobuf-ts/grpcweb-transport";
import { MapUpdateClient } from "../generated/grpc/services/MapUpdate.client";
import { IncrementalMapUpdateRequest, IncrementalMapUpdateRequest_IncrementalMapUpdate } from "../generated/grpc/services/MapUpdate";
import { PixelTypes } from "../generated/grpc/components/enums/pixelTypes";
import { PixelOwners } from "../generated/grpc/components/enums/pixelOwners";
import { RelativeCoordinate } from "../generated/grpc/components/schemas/relativeCoordinate";

export default function GRPCTest() {
  useEffect(() => {
    callGRPC();
  });

  const callGRPC = async () => {
    const transport = new GrpcWebFetchTransport({
      baseUrl: window.location.origin
    });
    const mapUpdateClient = new MapUpdateClient(transport)

    const spawnRelativeCoordinate: RelativeCoordinate = {
      x: 1,
      y: 1
    }
    const incrementalMapUpdate: IncrementalMapUpdateRequest_IncrementalMapUpdate = {
      type: PixelTypes.Normal,
      owner: PixelOwners.Cluster,
      ownPixel: true,
      spawnRelativeCoordinate: spawnRelativeCoordinate
    }
    const incrementalMapUpdateRequest: IncrementalMapUpdateRequest = {
      updates: [incrementalMapUpdate]
    }
    const response = await mapUpdateClient.getIncremental(incrementalMapUpdateRequest)
    response.status
  };

  return <div id="grpcdemo"></div>;
}
