import { User } from "../models/User.ts";
import { useQuery } from "@tanstack/react-query";
import { getCurrentUser } from "../api/users.ts";
import { useNavigate } from "react-router-dom";
import { AxiosError } from "axios";
import { useCallback } from "react";

export const useUser = (): User | null => {
    const navigate = useNavigate();

    const getUserWithRedirect = useCallback(async () => {
        try {
            return await getCurrentUser();
        } catch (error) {
            const axiosError = error as AxiosError;
            if (axiosError.response?.status === 401) {
                document.cookie = "";
                navigate("/");
            }
        }
    }, [navigate]);

    const { data, isSuccess } = useQuery({ queryKey: ["current_user"], queryFn: getUserWithRedirect });
    if (isSuccess) {
        return data?.data ?? null;
    }

    return null;
};
