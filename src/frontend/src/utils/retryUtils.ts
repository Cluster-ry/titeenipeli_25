export type RetrySettings = {
    standardRetryCount: number;
    standardRetryWaitInMilliseconds: number;
    backoutMethod: (attempt: number) => number;
};
const defaultRetrySettings: RetrySettings = {
    standardRetryCount: 3,
    standardRetryWaitInMilliseconds: 5000,
    backoutMethod: (attempt: number) => Math.max(attempt * 5000) + Math.floor(Math.random() * 2000),
};

export async function withRetry<T>(method: () => Promise<T>, settings: RetrySettings = defaultRetrySettings) {
    // Normal retry.
    for (let attempt = 0; attempt < settings.standardRetryCount; attempt++) {
        try {
            return await method();
        } catch (error) {
            console.log(error);
        }
        sleep(settings.standardRetryWaitInMilliseconds);
    }

    // Backout retry.
    let attempt = settings.standardRetryCount;
    // eslint-disable-next-line no-constant-condition
    while (true) {
        attempt++;
        try {
            return await method();
        } catch (error) {
            console.log(error);
        }
        const sleepTime = settings.backoutMethod(attempt);
        sleep(sleepTime);
    }
}

function sleep(milliseconds: number) {
    return new Promise((resolve) => setTimeout(resolve, milliseconds));
}

export default withRetry;
