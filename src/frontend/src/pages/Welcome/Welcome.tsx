import { PropsWithChildren, useEffect, useState } from "react";
import { LogoContainer } from "./LogoContainer";
import { GreetingContainer } from "./GreetingContainer";
import { useNavigate, useSearchParams } from "react-router-dom";
import { postUsersAuthenticate } from "../../api/users";

export const Welcome = () => {
  const navigate = useNavigate();
  async function login() {

    window.location.href = "https://web.telegram.org"
  }

  const [searchParams, setSearchParams] = useSearchParams()

  useEffect(() => {
    const token = searchParams.get("token")
    if (token === null) {
      return
    }
    handleToken(token)

    async function handleToken(token: string) {
      const authenticationInput = {token: token}
      try {
        const response = await postUsersAuthenticate(authenticationInput) 
        if (response.status === 200) {
          navigate("/game")
        }
      } catch (error) {
        setSearchParams({})
        console.log(error)
      }
    }
  }, []);

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
