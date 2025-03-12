import { FC, ReactNode, useCallback, useRef } from "react";
import "./modal.css";

interface ModalProps {
    title: string;
    children: ReactNode;
    onClose: () => void;
}

export const Modal: FC<ModalProps> = ({ title, children, onClose }) => {
    const modalRef = useRef<HTMLDivElement>(null);

    const onAnimationEnd = useCallback(() => {
        if (modalRef.current && modalRef.current.className === "modal-content closing") {
            modalRef.current.className = "modal-content opening";
            onClose();
        }
    }, [onClose]);

    const onCloseClick = useCallback(() => {
        modalRef.current && (modalRef.current.className = "modal-content closing");
    }, []);

    const stopEventPropagation = useCallback((event: React.MouseEvent<HTMLDivElement, MouseEvent>) => {
        event.stopPropagation();
    }, []);

    return (
        <div className="modal" onClick={onCloseClick}>
            <div ref={modalRef} onAnimationEnd={onAnimationEnd} className="modal-content opening" onClick={stopEventPropagation}>
                <div className="modal-header">
                    <button className="close" onClick={onCloseClick}>
                        &times;
                    </button>
                    <h2>{title}</h2>
                </div>
                <div className="modal-body">{children}</div>
            </div>
        </div>
    );
};

export default Modal;
