import GrpcClient from "./grpcClient";

export default abstract class ServerStreamingServiceClient<ResponseType> {
  private static reconnectDelaysMs = 3000;

  connected = false;

  protected onResponsecallbacks: Array<
    (response: ResponseType) => void | Promise<void>
  > = [];
  protected grpcClient: GrpcClient;

  constructor(grpcClient: GrpcClient) {
    this.grpcClient = grpcClient;
  }

  async connect() {
    while (true) {
      try {
        await this.connectService();
      } catch (error) {
        this.connected = false;
        this.grpcClient.handleErrorCallbacks(error);

        await new Promise((callback) =>
          setTimeout(callback, ServerStreamingServiceClient.reconnectDelaysMs)
        );
      }
    }
  }

  protected abstract connectService(): Promise<void>;

  protected async callCallbacks(response: ResponseType) {
    if (!this.connected) {
      this.connected = true;
      await this.grpcClient.handleOnConnectionStatusChangedCallbacks();
    }

    for (const onResponsecallback of this.onResponsecallbacks) {
      await onResponsecallback(response);
    }
  }

  registerOnResponseListener(
    callback: (response: ResponseType) => void | Promise<void>
  ) {
    this.onResponsecallbacks.push(callback);
  }

  unRegisterOnResponseListener(
    callback: (response: ResponseType) => void | Promise<void>
  ) {
    var index = this.onResponsecallbacks.indexOf(callback);
    this.onResponsecallbacks.splice(index, 1);
  }
}
