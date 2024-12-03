import { GrpcWebFetchTransport } from "@protobuf-ts/grpcweb-transport";
import IncrementalMapUpdateClient from "./incrementalMapUpdateClient";
import MiscStateUpdateClient from "./miscStateUpdateClient";

export class GrpcClients {
    static instance: GrpcClients | null = null;

    incrementalMapUpdateClient!: IncrementalMapUpdateClient;
    miscStateUpdateClient!: MiscStateUpdateClient;

    private transport: GrpcWebFetchTransport;

    private constructor() {
        this.transport = new GrpcWebFetchTransport({
            baseUrl: window.location.origin,
        });

        this.initializeGrpcClients();
    }

    private async initializeGrpcClients() {
        this.incrementalMapUpdateClient = new IncrementalMapUpdateClient(this.transport);
        this.miscStateUpdateClient = new MiscStateUpdateClient(this.transport);
    }

    public static getGrpcClients() {
        if (GrpcClients.instance === null) {
            GrpcClients.instance = new GrpcClients();
        }
        return GrpcClients.instance;
    }
}

export default GrpcClients;
