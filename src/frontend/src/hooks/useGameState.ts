import { MiscStateUpdateResponse } from "./../generated/grpc/services/StateUpdate";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useCallback, useEffect, useRef } from "react";
import { useNotificationStore } from "../stores/notificationStore.ts";
import GrpcClients from "../core/grpc/grpcClients.ts";
import { useGameStateStore } from "../stores/gameStateStore.ts";
import { getGameState } from "../api/gameState.ts";
import { AxiosResponse } from "axios";
import { GetGameState } from "../models/Get/GetGameState.ts";

const stateQueryKey = "gameState";

export const useGameState = () => {
    const queryClient = useQueryClient();
    const triggerNotification = useNotificationStore(state => state.triggerNotification);
    const grpcClient = useRef<GrpcClients>(GrpcClients.getGrpcClients());

    const setPixelBucket = useGameStateStore((state) => state.setPixelBucket);
    const setScores = useGameStateStore((state) => state.setScores);
    const setState = useGameStateStore((state) => state.setMiscGameState);
    const setPowerUps = useGameStateStore((state) => state.setPowerUps);

    const { data, isSuccess } = useQuery({
        queryKey: [stateQueryKey],
        queryFn: getGameState,
        refetchOnMount: "always",
        refetchOnReconnect: "always",
        staleTime: 0,
        gcTime: 0,
        retry: true,
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

            if (update.powerupUpdate) {
                const powerups = update.powerUps.map((powerup) => ({
                    powerUpId: powerup.powerUpId,
                    name: powerup.name,
                    description: powerup.description,
                    directed: powerup.directed,
                }));
                setPowerUps(powerups);
            }
      
            if(update.notification !== undefined){
                triggerNotification(update.notification.message, "neutral")
            }
            
        },
        [setPixelBucket, setScores, setPowerUps, triggerNotification],
    );

    const onIncrementalUpdate = useCallback(
        (incrementalUpdateResponse: MiscStateUpdateResponse) => {
            consumeUpdate(incrementalUpdateResponse);
        },
        [consumeUpdate],
    );

    const ensureState = useCallback(async () => {
        await queryClient.resetQueries({ queryKey: [stateQueryKey] });
    }, []);

    useEffect(() => {
        console.debug("Rendering useGameState hook. Success:", isSuccess);
        if (isSuccess) {
            const stateResponse = data as AxiosResponse<GetGameState>;
            setState(stateResponse.data);
        }
    }, [isSuccess, setState]);

    useEffect(() => {
        console.debug("Registering GRPC client for misc state updates");
        grpcClient.current.miscStateUpdateClient.registerOnResponseListener(onIncrementalUpdate);
        return () => {
            grpcClient.current.miscStateUpdateClient.unRegisterOnResponseListener(onIncrementalUpdate);
        };
    }, [onIncrementalUpdate, grpcClient]);

    return { ensureState };
};
