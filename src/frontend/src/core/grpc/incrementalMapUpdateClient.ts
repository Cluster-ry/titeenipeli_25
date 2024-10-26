import { MapUpdateClient } from "../../generated/grpc/services/MapUpdate.client";
import { IncrementalMapUpdateRequest, IncrementalMapUpdateResponse } from "../../generated/grpc/services/MapUpdate";
import ServerStreamingServiceClient from "./serverStreamingServiceClient";

export default class IncrementalMapUpdateClient extends ServerStreamingServiceClient<IncrementalMapUpdateResponse> {
    protected async connectService() {
        const mapUpdateClient = new MapUpdateClient(this.grpcClient.transport);

        const incrementalMapUpdateRequest: IncrementalMapUpdateRequest = {};
        const serverStreaming = mapUpdateClient.getIncremental(incrementalMapUpdateRequest);

        for await (const response of serverStreaming.responses) {
            await this.callCallbacks(response);
        }

        await serverStreaming.status;
    }
}
