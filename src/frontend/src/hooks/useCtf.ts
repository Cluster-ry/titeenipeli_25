import axios, { AxiosResponse } from "axios";
import { useMutation, useQuery } from "@tanstack/react-query";
import { postCtf } from "../api/ctf";
import { PostCtfInput } from "../models/Post/PostCtfInput";

export const useCtf = ():  | null => {
  
  // NOTE! IMPLEMENTATION COULD BE WORTH CHANGING IN THE FUTURE!
  const ctfToken: PostCtfInput = {
    token: "FGSTLBGXM3YB7USWS28KE2JV9Z267L48"
  };
 
  const mutation = useMutation({
    mutationFn(ctfToken) => {
      postCtf(ctfToken);
    },
  });

  const { data, isSuccess } = useQuery({ queryKey: ["current_user"], queryFn: postCtf });
  if (isSuccess) {
    return data.data;
  }
  return null;
}
