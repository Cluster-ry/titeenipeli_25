import { useErrorStore } from "../stores/errorStore";

const { updateShowError, startErrorTimer } = useErrorStore();

export const showErrorNotification = () => {
  updateShowError(true);
  startErrorTimer();
}
