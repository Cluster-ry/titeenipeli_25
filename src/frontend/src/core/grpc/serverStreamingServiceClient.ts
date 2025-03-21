import CommonGrpcClient from "./commonGrpcClient";

export default abstract class ServerStreamingServiceClient<ResponseType> extends CommonGrpcClient {
    private static reconnectDelaysMs = 3000;

    protected onResponsecallbacks: Array<(response: ResponseType) => void | Promise<void>> = [];

    async connect() {
        // eslint-disable-next-line no-constant-condition
        while (true) {
            try {
                await this.connectService();
            } catch (error) {
                this.connected = false;
                this.handleErrorCallbacks(error);

                await new Promise((callback) => setTimeout(callback, ServerStreamingServiceClient.reconnectDelaysMs));
            }
        }
    }

    protected abstract connectService(): Promise<void>;

    protected async callCallbacks(response: ResponseType) {
        if (!this.connected) {
            this.connected = true;
            await this.handleOnConnectionStatusChangedCallbacks();
        }

        for (const onResponsecallback of this.onResponsecallbacks) {
            await onResponsecallback(response);
        }
    }

    registerOnResponseListener(callback: (response: ResponseType) => void | Promise<void>) {
        this.onResponsecallbacks.push(callback);
    }

    unRegisterOnResponseListener(callback: (response: ResponseType) => void | Promise<void>) {
        const index = this.onResponsecallbacks.indexOf(callback);
        this.onResponsecallbacks.splice(index, 1);
    }
}
