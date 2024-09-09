export interface ClientApiError {
    msg: string;
}

export function instanceOfClientApiError(data: any): data is ClientApiError { 
    return 'msg' in data; 
} 