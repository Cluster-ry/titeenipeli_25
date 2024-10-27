import { StateUpdateClient } from "../../generated/grpc/services/StateUpdate.client";
import { IncrementalMapUpdateRequest, IncrementalMapUpdateResponse } from "../../generated/grpc/services/StateUpdate";
import ServerStreamingServiceClient from "./serverStreamingServiceClient";

export default class IncrementalMapUpdateClient extends ServerStreamingServiceClient<IncrementalMapUpdateResponse> {
    protected async connectService() {
        const mapUpdateClient = new StateUpdateClient(this.grpcClient.transport);

        const incrementalMapUpdateRequest: IncrementalMapUpdateRequest = {};
        const serverStreaming = mapUpdateClient.getIncrementalMapUpdate(incrementalMapUpdateRequest);

        for await (const response of serverStreaming.responses) {
            await this.callCallbacks(response);
        }

        await serverStreaming.status;
    }
}
