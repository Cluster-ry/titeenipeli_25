import { GrpcWebFetchTransport } from "@protobuf-ts/grpcweb-transport";
import IncrementalMapUpdateClient from "./incrementalMapUpdateClient";

export class GrpcClient {
  static instance = new GrpcClient();

  transport: GrpcWebFetchTransport;

  incrementalMapUpdateClient: IncrementalMapUpdateClient | undefined;

  private onConnectionStatusChangedCallbacks: Array<
    (connected: boolean) => Promise<void> | void
  > = [];
  private onErrorCallbacks: Array<(error: unknown) => Promise<void> | void> =
    [];

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
    let newConnectionStatus =
      this.incrementalMapUpdateClient?.connected ?? false;

    if (newConnectionStatus !== this.previousOverallConnectionStatus) {
      this.previousOverallConnectionStatus = newConnectionStatus;
      for (const onConnectionStatusChangedCallback of this
        .onConnectionStatusChangedCallbacks) {
        await onConnectionStatusChangedCallback(newConnectionStatus);
      }
    }
  }

  registerOnConnectionStatusChangedListener(
    callback: (connected: boolean) => Promise<void> | void
  ) {
    this.onConnectionStatusChangedCallbacks.push(callback);
  }

  unRegisterOnConnectionStatusChangedListener(
    callback: (connected: boolean) => Promise<void> | void
  ) {
    var index = this.onConnectionStatusChangedCallbacks.indexOf(callback);
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
    var index = this.onErrorCallbacks.indexOf(callback);
    this.onErrorCallbacks.splice(index, 1);
  }
}

export function getGrpcClient() {
  return GrpcClient.instance;
}

export default GrpcClient;
