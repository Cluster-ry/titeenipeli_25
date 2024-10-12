import { PropsWithChildren } from "react";
import { LogoContainer } from "./LogoContainer";
import { GreetingContainer } from "./GreetingContainer";
import { postLogin } from "../../api/login";
import { useNavigate } from "react-router-dom";

export const Welcome = () => {
  const navigate = useNavigate();
  async function login() {
    await postLogin({ username: "test", password: "test123" });
    /*await postUsers({
      id: "1",
      firstName: "test",
      lastName: "test",
      username: "test",
      photoUrl: "",
      authDate: "",
      hash: "",
    });*/
    navigate("/game");
  }

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
          <GreetingContainer login={login} />
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
