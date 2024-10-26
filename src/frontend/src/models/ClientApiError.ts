export interface ClientApiError {
    msg: string;
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function instanceOfClientApiError(data: any): data is ClientApiError {
    return "msg" in data;
}
