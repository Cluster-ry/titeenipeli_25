import { ReactNode } from "react";
import "./modal.css";

interface ModalProps {
    title: string;
    children: ReactNode;
    onClose: () => void;
}

export const Modal = ({ title, children, onClose }: ModalProps) => {
    return (
        <div className="modal">
            <div className="modal-content">
                <div className="modal-header">
                    <button className="close" onClick={onClose}>
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
