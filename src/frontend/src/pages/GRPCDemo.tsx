import { useState } from "react";
import { IncrementalMapUpdateResponse } from "../generated/grpc/services/MapUpdate";
import { GrpcClient } from "../core/grpc/grpcClient";

const GRPCTest = () => {
    const [grpcConnectionStatus, setGrpcConnectionStatus] = useState(false);
    const [latestGrpcResponse, setLatestGrpcResponse] = useState<IncrementalMapUpdateResponse>();

    const grpcClient = GrpcClient.getGrpcClient();
    grpcClient.registerOnConnectionStatusChangedListener(setGrpcConnectionStatus);
    grpcClient.incrementalMapUpdateClient?.registerOnResponseListener(setLatestGrpcResponse);

    return (
        <div id="grpcdemo">
            <p>Connection status: {grpcConnectionStatus.toString()}</p>
            <p>Response: {JSON.stringify(latestGrpcResponse)}</p>
        </div>
    );
};

export default GRPCTest;
