import { PropsWithChildren } from "react";
import background_logo from "../../assets/logo_background.png";
import logo from "../../assets/logo.png";

export const LogoContainer = () => {
    return (
        <div
            style={{
                position: "relative",
                display: "flex",
                flexDirection: "column",
                alignItems: "center",
                justifyItems: "center",
                height: "100%",
                width: "100%",
                borderRadius: "10px",
                outline: "2px dashed #000000",
                overflow: "clip",
            }}
        >
            <ImageContainer>
                <img
                    style={{
                        position: "absolute",
                        top: 0,
                        left: 0,
                        right: 0,
                        bottom: 0,
                        minWidth: "100%",
                        maxHeight: "100%",
                    }}
                    src={background_logo}
                />
                <img
                    style={{
                        position: "absolute",
                        top: 0,
                        left: 0,
                        right: 0,
                        bottom: 0,
                        margin: "auto",
                        maxHeight: "100%",
                    }}
                    src={logo}
                />
            </ImageContainer>
            <Overlay />
            <div
                style={{
                    display: "flex",
                    flexDirection: "column",
                    alignItems: "center",
                    justifyItems: "end",
                    zIndex: 1,
                }}
            >
                <span>Clash of the Titens</span>
                <span>2025</span>
            </div>
        </div>
    );
};

const ImageContainer = ({ children }: PropsWithChildren) => {
    return (
        <div
            style={{
                position: "relative",
                width: "100%",
                height: "100%",
            }}
        >
            {children}
        </div>
    );
};

const Overlay = () => {
    return (
        <div
            style={{
                position: "absolute",
                top: 0,
                left: 0,
                right: 0,
                bottom: 0,
                background:
                    "linear-gradient(180deg, rgba(0, 0, 0, 0.2) 0%, rgba(183, 183, 183, 0.874667) 74.83%, #D9D9D9 100%)",
            }}
        />
    );
};
