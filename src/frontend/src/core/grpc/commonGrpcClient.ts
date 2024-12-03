import { GrpcWebFetchTransport } from "@protobuf-ts/grpcweb-transport";

export abstract class CommonGrpcClient {
    protected connected = false;

    private onConnectionStatusChangedCallbacks: Array<(connected: boolean) => Promise<void> | void> = [];
    private onErrorCallbacks: Array<(error: unknown) => Promise<void> | void> = [];

    private previousOverallConnectionStatus = false;

    constructor(protected transport: GrpcWebFetchTransport) {
        this.connect();
    }

    protected abstract connect(): Promise<void>;

    async handleOnConnectionStatusChangedCallbacks() {
        const newConnectionStatus = this.connected ?? false;

        if (newConnectionStatus !== this.previousOverallConnectionStatus) {
            this.previousOverallConnectionStatus = newConnectionStatus;
            for (const onConnectionStatusChangedCallback of this.onConnectionStatusChangedCallbacks) {
                await onConnectionStatusChangedCallback(newConnectionStatus);
            }
        }
    }

    registerOnConnectionStatusChangedListener(callback: (connected: boolean) => Promise<void> | void) {
        this.onConnectionStatusChangedCallbacks.push(callback);
        callback(this.connected ?? false);
    }

    unRegisterOnConnectionStatusChangedListener(callback: (connected: boolean) => Promise<void> | void) {
        const index = this.onConnectionStatusChangedCallbacks.indexOf(callback);
        this.onConnectionStatusChangedCallbacks.splice(index, 1);
    }

    async handleErrorCallbacks(error: unknown) {
        await this.handleOnConnectionStatusChangedCallbacks();
        for (const onErrorCallback of this.onErrorCallbacks) {
            await onErrorCallback(error);
        }
    }

    registerOnErrorListener(callback: (error: unknown) => Promise<void>) {
        this.onErrorCallbacks.push(callback);
    }

    unRegisterOnErrorListener(callback: (error: unknown) => Promise<void>) {
        const index = this.onErrorCallbacks.indexOf(callback);
        this.onErrorCallbacks.splice(index, 1);
    }
}

export default CommonGrpcClient;
