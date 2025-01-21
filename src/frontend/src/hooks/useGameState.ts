import { MiscStateUpdateResponse } from "./../generated/grpc/services/StateUpdate";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useCallback, useEffect, useRef } from "react";
import GrpcClients from "../core/grpc/grpcClients.ts";
import { useGameStateStore } from "../stores/gameStateStore.ts";
import { getGameState } from "../api/gameState.ts";
import { AxiosResponse } from "axios";
import { GetGameState } from "../models/Get/GetGameState.ts";

const stateQueryKey = "gameState";

export const useGameState = () => {
    const queryClient = useQueryClient();
    const grpcClient = useRef<GrpcClients>(GrpcClients.getGrpcClients());

    const setPixelBucket = useGameStateStore((state) => state.setPixelBucket);
    const setScores = useGameStateStore((state) => state.setScores);
    const setState = useGameStateStore((state) => state.setMiscGameState);
    const setPowerUps = useGameStateStore((state) => state.setPowerUps);
    
    const { data, isSuccess, status } = useQuery({
        queryKey: [stateQueryKey],
        queryFn: getGameState,
        refetchOnReconnect: "always",
        refetchOnMount: "always",
    });

    const consumeUpdate = useCallback(
        (update: MiscStateUpdateResponse) => {
            if (update.pixelBucket !== undefined) {
                const pixelBucket = update.pixelBucket;
                setPixelBucket(pixelBucket);
            }

            if (update.scores.length > 0) {
                const scores = update.scores.map((score) => ({ guild: Number(score.guild), amount: score.amount }));
                setScores(scores);
            }
            
            if(update.powerupUpdate) {
                const powerups = update.powerUps.map((powerup) => ({ powerUpId: powerup.powerUpId,  name: powerup.name, description : powerup.description, directed: powerup.directed }));
                setPowerUps(powerups);
            }
            
        },
        [setPixelBucket, setScores, setPowerUps],
    );

    const onIncrementalUpdate = useCallback(
        (incrementalUpdateResponse: MiscStateUpdateResponse) => {
            consumeUpdate(incrementalUpdateResponse);
        },
        [isSuccess, data, status],
    );

    const ensureState = useCallback(async () => {
        await queryClient.resetQueries({ queryKey: [stateQueryKey] });
    }, []);

    useEffect(() => {
        console.log("Rendering useGameState hook. Success:", isSuccess);
        if (isSuccess) {
            const stateResponse = data as AxiosResponse<GetGameState>;
            setState(stateResponse.data);
        }
    }, [isSuccess, setState]);

    useEffect(() => {
        console.log("Registering GRPC client");
        grpcClient.current.miscStateUpdateClient.registerOnResponseListener(onIncrementalUpdate);
        return () => {
            grpcClient.current.miscStateUpdateClient.unRegisterOnResponseListener(onIncrementalUpdate);
        };
    }, [onIncrementalUpdate]);

    return { ensureState };
};
