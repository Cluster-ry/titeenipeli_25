import { PropsWithChildren } from "react";
import { LogoContainer } from "./LogoContainer";
import { GreetingContainer } from "./GreetingContainer";

export const Welcome = () => {
  return (
    <>
      <VerticalHalfContainer>
        <LogoContainer />
      </VerticalHalfContainer>
      <VerticalHalfContainer>
        <div
          style={{
            height: "95%",
            paddingLeft: "10vw",
            paddingRight: "10vw",
            paddingTop: "1vh",
          }}
        >
          <GreetingContainer />
        </div>
      </VerticalHalfContainer>
      AppVersion
    </>
  );
};

const VerticalHalfContainer = ({ children }: PropsWithChildren) => {
  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        height: "50%",
      }}
    >
      {children}
    </div>
  );
};
