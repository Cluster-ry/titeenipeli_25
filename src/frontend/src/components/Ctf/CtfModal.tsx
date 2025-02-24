import {useEffect, useState} from "react";
import { postCtf } from "../../api/ctf";
import { PostCtfInput } from "../../models/Post/PostCtfInput";
import "../../pages/Game/Overlay/overlay.css";
import { useNotificationStore } from "../../stores/notificationStore";
import "./ctf.css";
import Modal from "../Modal/Modal";
import { useCtfStore } from "../../stores/ctfModalStore";
import {CtfOk} from "../../models/CtfOk.ts";
import {setRandomInterval} from "../../utils/setRandomInterval.ts";

const CtfModal = () => {
    const [token, setToken] = useState("");
    const { setCtfModelOpenState } = useCtfStore();
    const { triggerNotification } = useNotificationStore();

    const CTF_DISCLAIMER: string = "Activate CTF";
    const NOTIFICATION_FAIL: string = "CTF activation failed.";

    const createSuccessNotificationText = (result: CtfOk) => {
        return `${result.title}\n${result.message}\nBenefits:${result.benefits.join('\n')}`;
    };

    useEffect(() => {
        "#OH_YOU_FOUND_THIS?";
        setRandomInterval(() => console.log("#ARE_YOU_SURE?"), 120e3, 300e3);
    }, []);

    const handleTokenChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setToken(event.target.value);
    };

    const handleSubmit = async () => {
        const ctfToken: PostCtfInput = {
            token: token,
        };
        try {
            const result = await postCtf(ctfToken);
            if ("msg" in result && result.msg === "Request unsuccessful.") {
                triggerNotification(NOTIFICATION_FAIL, "error");
                console.debug("Request unsuccessful.");
                return;
            } else if ("data" in result) {
                triggerNotification(createSuccessNotificationText(result.data), "success");
            }
        } catch (error) {
            triggerNotification(NOTIFICATION_FAIL, "error");
            console.error(error);
        }
    };

    return (
        <Modal title="CTF input" onClose={() => setCtfModelOpenState(false)}>
            <input
                className="ctf-input"
                type="text"
                placeholder="Enter Token"
                value={token}
                onChange={handleTokenChange}
                style={{ pointerEvents: "all" }}
            />
            <button className="ctf-button" onClick={handleSubmit}>
                {CTF_DISCLAIMER}
            </button>
        </Modal>
    );
};

export default CtfModal;
