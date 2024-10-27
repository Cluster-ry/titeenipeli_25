import { GrpcWebFetchTransport } from "@protobuf-ts/grpcweb-transport";
import IncrementalMapUpdateClient from "./incrementalMapUpdateClient";

export class GrpcClient {
    static instance: GrpcClient | null = null;

    transport: GrpcWebFetchTransport;

    incrementalMapUpdateClient: IncrementalMapUpdateClient | undefined;

    private onConnectionStatusChangedCallbacks: Array<(connected: boolean) => Promise<void> | void> = [];
    private onErrorCallbacks: Array<(error: unknown) => Promise<void> | void> = [];

    private previousOverallConnectionStatus = false;

    private constructor() {
        this.transport = new GrpcWebFetchTransport({
            baseUrl: window.location.origin,
        });

        this.initializeGrpcStreams();
    }

    private async initializeGrpcStreams() {
        this.incrementalMapUpdateClient = new IncrementalMapUpdateClient(this);
        this.incrementalMapUpdateClient.connect();
    }

    async handleOnConnectionStatusChangedCallbacks() {
        const newConnectionStatus = this.incrementalMapUpdateClient?.connected ?? false;

        if (newConnectionStatus !== this.previousOverallConnectionStatus) {
            this.previousOverallConnectionStatus = newConnectionStatus;
            for (const onConnectionStatusChangedCallback of this.onConnectionStatusChangedCallbacks) {
                await onConnectionStatusChangedCallback(newConnectionStatus);
            }
        }
    }

    registerOnConnectionStatusChangedListener(callback: (connected: boolean) => Promise<void> | void) {
        this.onConnectionStatusChangedCallbacks.push(callback);
        callback(this.incrementalMapUpdateClient?.connected ?? false);
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

    public static getGrpcClient() {
        if (GrpcClient.instance === null) {
            GrpcClient.instance = new GrpcClient();
        }
        return GrpcClient.instance;
    }
}

export default GrpcClient;
