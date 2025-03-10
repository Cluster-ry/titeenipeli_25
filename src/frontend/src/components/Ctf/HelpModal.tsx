import { useCallback, useEffect, useState } from "react";
import { postCtf } from "../../api/ctf";
import { PostCtfInput } from "../../models/Post/PostCtfInput";
import "../../pages/Game/Overlay/overlay.css";
import { useNotificationStore } from "../../stores/notificationStore";
import "./ctf.css";
import Modal from "../Modal/Modal";
import { useHelpModalStore } from "../../stores/helpModalStore.ts";
import { CtfOk } from "../../models/CtfOk.ts";
import { setRandomInterval } from "../../utils/setRandomInterval.ts";
import { useInstructionsStore } from "../../stores/instructionsStore.ts";
import GraphicsSwitch from "../GraphicsSwitch/GraphicsSwitch.tsx";

const HelpModal = () => {
    const [token, setToken] = useState("");
    const setHelpModalOpenState = useHelpModalStore(state => state.setHelpModalOpenState);
    const triggerNotification = useNotificationStore(state => state.triggerNotification);
    const setInstructionsOn = useInstructionsStore(state => state.setInstructionsOn);

    const CTF_DISCLAIMER: string = "Activate CTF";
    const NOTIFICATION_FAIL: string = "CTF activation failed.";

    const createSuccessNotificationText = (result: CtfOk) => {
        return `${result.title}\n${result.message}\nBenefits:${result.benefits.join("\n")}`;
    };

    useEffect(() => {
        "#OH_YOU_FOUND_THIS?";
        setRandomInterval(() => console.log("#ARE_YOU_SURE?"), 120e3, 300e3);
    }, [setRandomInterval]);

    const handleTokenChange = useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        setToken(event.target.value);
    }, [setToken]);

    const handleSubmit = useCallback(async () => {
        const ctfToken: PostCtfInput = {
            token,
        };
        try {
            const result = await postCtf(ctfToken);
            console.debug(result);
            if ("msg" in result) {
                triggerNotification(result.msg, "error");
                console.debug("Request unsuccessful.");
                return;
            } else if ("data" in result) {
                triggerNotification(createSuccessNotificationText(result.data), "success");
            }
        } catch (error) {
            triggerNotification(NOTIFICATION_FAIL, "error");
            console.error(error);
        }
    }, [triggerNotification, token]);

    const openInstructionsModal = useCallback(() => {
        setInstructionsOn(true);
    }, [setInstructionsOn]);

    const closeHelpModal = useCallback(() => {
        setHelpModalOpenState(false);
    }, [setHelpModalOpenState]);

    return (
        <Modal title="CTF input" onClose={closeHelpModal}>
            <div className="ctf-input-wrapper">
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

                <div className="instructions">
                    <button className="modal-button" onClick={openInstructionsModal}>
                        Check instructions
                    </button>
                </div>
                <GraphicsSwitch />
            </div>
        </Modal>
    );
};

export default HelpModal;
