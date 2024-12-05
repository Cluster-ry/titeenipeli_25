import { StateUpdateClient } from "../../generated/grpc/services/StateUpdate.client";
import { IncrementalMapUpdateRequest, MiscStateUpdateResponse } from "../../generated/grpc/services/StateUpdate";
import ServerStreamingServiceClient from "./serverStreamingServiceClient";

export default class MiscStateUpdateClient extends ServerStreamingServiceClient<MiscStateUpdateResponse> {
    protected async connectService() {
        const miscStateUpdateClient = new StateUpdateClient(this.transport);

        const incrementalMapUpdateRequest: IncrementalMapUpdateRequest = {};
        const serverStreaming = miscStateUpdateClient.getMiscGameStateUpdate(incrementalMapUpdateRequest);

        for await (const response of serverStreaming.responses) {
            await this.callCallbacks(response);
        }

        await serverStreaming.status;
    }
}
