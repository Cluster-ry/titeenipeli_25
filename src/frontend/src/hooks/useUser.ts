import { User } from "../models/User.ts";
import { useQuery } from "@tanstack/react-query";
import { getCurrentUser } from "../api/users.ts";

export const useUser = (): User | null => {
    const { data, isSuccess } = useQuery({ queryKey: ["current_user"], queryFn: getCurrentUser });
    if (isSuccess) {
        return data.data;
    }
    return null;
};
