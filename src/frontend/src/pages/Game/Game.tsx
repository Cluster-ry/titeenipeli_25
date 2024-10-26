import { ReactNode, useMemo } from "react";
import { GameOverlay } from "./Overlay/GameOverlay";
import { useQuery } from "@tanstack/react-query";
import "./game.css";
import { getCurrentUser } from "../../api/users.ts";
import { useUserStore } from "../../stores/userStore.ts";

type GameProps = {
    slot: ReactNode;
};

const Game = ({ slot }: GameProps) => {
    const { data, isSuccess } = useQuery({ queryKey: ["current_user"], queryFn: getCurrentUser });
    const { user, setUser } = useUserStore();
    useMemo(() => {
        if (isSuccess) {
            setUser(data.data);
            console.log("Current user:", user);
        }
    }, [data, isSuccess]);
    return (
        <div className="game-container">
            <div className="slot-container">{slot}</div>
            <GameOverlay />
        </div>
    );
};

export { Game };
