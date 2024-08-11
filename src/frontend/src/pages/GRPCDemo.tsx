import { useState } from "react";
import { IncrementalMapUpdateResponse } from "../generated/grpc/services/MapUpdate";
import { getGrpcClient } from "../core/grpc/grpcClient";

export default function GRPCTest() {
  const [grpcConnectionStatus, setGrpcConnectionStatus] = useState(false);
  const [latestGrpcResponse, setLatestGrpcResponse] =
    useState<IncrementalMapUpdateResponse>();

  const grpcClient = getGrpcClient();
  grpcClient.registerOnConnectionStatusChangedListener(setGrpcConnectionStatus);
  grpcClient.incrementalMapUpdateClient?.registerOnResponseListener(
    setLatestGrpcResponse
  );

  return (
    <div id="grpcdemo">
      <p>Connection status: {grpcConnectionStatus.toString()}</p>
      <p>Response: {JSON.stringify(latestGrpcResponse)}</p>
    </div>
  )
}
